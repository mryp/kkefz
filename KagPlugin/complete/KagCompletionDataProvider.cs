﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using KagContext.parse;
using ScintillaNet;
using PluginCore;
using System.Drawing;
using System.Reflection;

namespace KagContext.complete
{
	/// <summary>
	/// KAG入力補完プロバイダクラス
	/// </summary>
	public class KagCompletionDataProvider
	{
		#region 定数
		/// <summary>
		/// ループ限界回数（主に再帰チェックに使用する）
		/// </summary>
		const int MAX_LOOP_COUNT = 20;
		#endregion

		#region フィールド
		private Bitmap m_iconTagKag;
		private Bitmap m_iconTagKagex;
		private Bitmap m_iconTagUser;
		private Bitmap m_iconAttrName;
		private Bitmap m_iconAttrValue;
		private Bitmap m_iconUnknown;

		/// <summary>
		/// 循環参照（＊とか）でチェックしカウントする変数
		/// </summary>
		int m_reverseCount = 0;

		/// <summary>
		/// カーソル直前の単語
		/// </summary>
		string m_preSelection = null;

		/// <summary>
		/// 属性値補完クラス
		/// </summary>
		KagCompletionAttrValue m_compAttrValue;
		#endregion

		#region プロパティ
		/// <summary>
		/// 入力補完時に選択状態にして置換する文字列
		/// </summary>
		public string PreSelection
		{
			get { return m_preSelection; }
		}
		#endregion

		#region 初期化終了処理
		public KagCompletionDataProvider()
		{
			//TODO: アイコンは暫定
			m_iconTagKag = getResourceImage("Icon.Property.png");
			m_iconTagKagex = getResourceImage("Icon.IPropertyProtected.png");
			m_iconTagUser = getResourceImage("Icon.PropertyPrivate.png");
			m_iconAttrName = getResourceImage("Icon.Method.png");
			m_iconAttrValue = getResourceImage("Icon.Variable.png");
			m_iconUnknown = getResourceImage("Icon.Package.png");

			m_compAttrValue = new KagCompletionAttrValue(m_iconAttrValue);
		}

		private Bitmap getResourceImage(string resourceID)
		{
			Image image;
			try
			{
				resourceID = "KagPlugin." + resourceID;
				Assembly assembly = Assembly.GetExecutingAssembly();
				string[] resources = assembly.GetManifestResourceNames();
				image = new Bitmap(assembly.GetManifestResourceStream(resourceID));
			}
			catch
			{
				image = new Bitmap(16, 16);
			}

			return (Bitmap)image;
		}
		#endregion

		#region 入力補完文字列関連
		/// <summary>
		/// KAG入力補完リストに表示するアイテムリストを返す
		/// </summary>
		/// <param name="fileName">ファイル名</param>
		/// <param name="textArea">アクティブなテキストエディタ</param>
		/// <param name="charTyped">入力された文字列</param>
		/// <returns>入力補完アイテムリスト</returns>
		public List<ICompletionListItem> GenerateCompletionData(ScintillaControl sci, char charTyped)
		{
			KagMacro[] macroList = PluginMain.ParserSrv.GetKagMacroList();
			if (macroList.Length == 0)
			{
				return null;	//一つもないときはリストを表示しない
			}

			string lineText = KagUtility.GetKagLineText(sci, sci.CurrentPos);
			if (KagUtility.ExistLineHead(lineText, ';'))
			{
				return null;	//コメント行のとき
			}
			if (KagUtility.ExistLineHead(lineText, '*'))
			{
				return null;	//ラベル行のとき
			}

			m_preSelection = null;
			List<ICompletionListItem> list = null;
			KagTagKindInfo info = KagUtility.GetTagKind(lineText);
			if (charTyped == '[')		//タグ名？
			{
				if (KagUtility.ExistLineHead(lineText, '@'))
				{
					return null;	//@タグがある
				}
				if (info.StrMode)
				{
					return null;	//文字列表示中
				}

				//タグ入力
				list = getMacroNameList(info, macroList, "", "");
			}
			else if (charTyped == '@')	//タグ名？
			{
				if (!KagUtility.ExistLineHead(lineText, '@'))
				{
					return null;    //@タグがない
				}
				if (lineText.Trim(new char[] {' ', '\0', '\t'}) != "@")
				{
					return null;	//@タグではない
				}
				if (info.StrMode)
				{
					return null;
				}

				//タグ入力
				list = getMacroNameList(info, macroList, "", "");
			}
			else if (charTyped == ' ')	//属性名？
			{
				if ((info.Kind == KagTagKindInfo.KagKind.Unknown)
				|| (info.StrMode == true))
				{
					return null;	//属性名ではないので何もしない
				}

				//属性名
				list = getAttrNameList(info, macroList);
			}
			else if (charTyped == '=')	//属性値？
			{
				if (info.Kind != KagTagKindInfo.KagKind.AttrValue)
				{
					return null;	//属性値ではないので何もしない
				}

				//属性値
				list = getAttrValueList(info, macroList);
			}
			else if (charTyped == '\0')	//その他（Ctrl+Space）
			{
				switch (info.Kind)
				{
					case KagTagKindInfo.KagKind.KagTagName:
						if (info.StrMode)
						{
							return null;
						}

						//タグ入力
						list = getMacroNameList(info, macroList, "", "");
						break;
					case KagTagKindInfo.KagKind.Unknown:
						if (info.StrMode)
						{
							return null;
						}
						//タグ入力
						list = getMacroNameList(info, macroList, "[", "]");
						break;
					case KagTagKindInfo.KagKind.AttrName:
						if (info.StrMode)
						{
							return null;
						}
						//属性入力
						list = getAttrNameList(info, macroList);
						break;
					case KagTagKindInfo.KagKind.AttrValue:
						//属性値入力
						list = getAttrValueList(info, macroList);
						break;
					default:
						return null;
				}

				//Debug.WriteLine("info=" + info.ToString());
				m_preSelection = getCompReplaceText(sci, info);
			}
			else
			{
				return null;	//何も表示しない
			}

			if (list == null || list.Count == 0)
			{
				return null;	//一つもないとき
			}
			return list;
		}

		/// <summary>
		/// 種類から対応するアイコン画像を返す
		/// </summary>
		/// <param name="kind"></param>
		/// <returns></returns>
		private Bitmap getIconBitmapFromTagKind(KagTagKindInfo.KagKind kind)
		{
			Bitmap bmp;
			switch (kind)
			{
				case KagTagKindInfo.KagKind.KagTagName:
					bmp = m_iconTagKag;
					break;
				case KagTagKindInfo.KagKind.KagexTagName:
					bmp = m_iconTagKagex;
					break;
				case KagTagKindInfo.KagKind.UserTagName:
					bmp = m_iconTagUser;
					break;
				case KagTagKindInfo.KagKind.AttrName:
					bmp = m_iconAttrName;
					break;
				case KagTagKindInfo.KagKind.AttrValue:
					bmp = m_iconAttrValue;
					break;
				case KagTagKindInfo.KagKind.Unknown:
				default:
					bmp = m_iconUnknown;
					break;
			}

			return bmp;
		}
		#endregion

		#region マクロ・タグ名リスト取得関連
		/// <summary>
		/// マクロ名リストを取得する
		/// </summary>
		/// <param name="info">現在位置の入力情報</param>
		/// <param name="macroList">マクロ情報リスト</param>
		/// <returns>マクロ名リスト</returns>
		List<ICompletionListItem> getMacroNameList(KagTagKindInfo info, KagMacro[] macroList, string prefix, string postfix)
		{
			List<ICompletionListItem> list = new List<ICompletionListItem>();
			foreach (KagMacro macro in macroList)
			{
				list.Add(new KagCompletionListItem(macro.Name, prefix + macro.Name + postfix, macro.Comment, getIconBitmapFromTagKind(getTagMacroKind(macro))));
			}

			return list;
		}

		/// <summary>
		/// KAGマクロの情報から入力補完用マクロの種類を返す
		/// </summary>
		/// <param name="macro"></param>
		/// <returns></returns>
		KagTagKindInfo.KagKind getTagMacroKind(KagMacro macro)
		{
			KagTagKindInfo.KagKind kind = KagTagKindInfo.KagKind.KagTagName;	//デフォルトをセット
			switch (macro.DefType)
			{
				case KagMacro.DefineType.Kag:
					kind = KagTagKindInfo.KagKind.KagTagName;
					break;
				case KagMacro.DefineType.Kagex:
					kind = KagTagKindInfo.KagKind.KagexTagName;
					break;
				case KagMacro.DefineType.User:
					kind = KagTagKindInfo.KagKind.UserTagName;
					break;
			}

			return kind;
		}
		#endregion

		#region 属性名リスト取得関連
		/// <summary>
		/// 属性名リストを取得する
		/// </summary>
		/// <param name="info">現在位置の入力情報</param>
		/// <param name="macroList">マクロ情報リスト</param>
		/// <returns>マクロ名リスト</returns>
		List<ICompletionListItem> getAttrNameList(KagTagKindInfo info, KagMacro[] macroList)
		{
			KagMacro macro = KagUtility.GetKagMacro(info.TagName, macroList);
			if (macro == null)
			{
				return null;	//マクロが見つからない
			}

			//属性を取得
			resetReverseCount();
			Dictionary<string, ICompletionListItem> table = new Dictionary<string, ICompletionListItem>();
			table = getAttrNameListFromMacro(macro, macroList, table);

			//すでに書いているものは削除する
			foreach (string deleteAttrName in info.AttrTable.Keys)
			{
				table.Remove(deleteAttrName);
			}

			//出力用にリストを変換
			List<ICompletionListItem> list = new List<ICompletionListItem>(table.Count);
			foreach (ICompletionListItem data in table.Values)
			{
				list.Add(data);
			}
			return list;
		}

		/// <summary>
		/// マクロオブジェクトから属性名リストを取得する
		/// ＊再帰する
		/// </summary>
		/// <param name="macro">取得するマクロオブジェクト</param>
		/// <param name="macroList">マクロ情報リスト</param>
		/// <param name="table">取得した属性名を格納するテーブル</param>
		/// <returns>取得した属性名テーブル（tableをそのまま返す）</returns>
		private Dictionary<string, ICompletionListItem> getAttrNameListFromMacro(KagMacro macro
			, KagMacro[] macroList, Dictionary<string, ICompletionListItem> table)
		{
			if (overcheckReverseCount())
			{
				//再帰回数がオーバーしているときは何もせずに終了する
				return table;
			}

			//通常のマクロ属性を追加
			foreach (KagMacroAttr attr in macro.AttrTable.Values)
			{
				if (table.ContainsKey(attr.Name) == false)
				{
					//存在しないときだけ追加する
					table.Add(attr.Name, new KagCompletionListItem(attr.Name, attr.Name, attr.Comment, getIconBitmapFromTagKind(KagTagKindInfo.KagKind.AttrName)));
				}
			}

			//全省略マクロ属性を追加
			KagMacro asterMacro = null;
			foreach (string macroName in macro.AsteriskTagList)
			{
				asterMacro = KagUtility.GetKagMacro(macroName, macroList);
				if (asterMacro == null)
				{
					continue;	//このマクロは飛ばす
				}

				//自分自身を呼び出し
				table = getAttrNameListFromMacro(asterMacro, macroList, table);
			}

			return table;
		}
		#endregion

		#region 属性値リスト取得関連
		/// <summary>
		/// 属性値リストを取得する
		/// </summary>
		/// <param name="macro">取得するマクロオブジェクト</param>
		/// <param name="macroList">マクロ情報リスト</param>
		/// <returns>属性値リスト</returns>
		private List<ICompletionListItem> getAttrValueList(KagTagKindInfo info, KagMacro[] macroList)
		{
			KagMacro macro = KagUtility.GetKagMacro(info.TagName, macroList);
			if (macro == null)
			{
				return null;	//マクロが見つからない
			}
			resetReverseCount();
			KagMacroAttr attr = getMacroAttr(info.AttrName, macro, macroList);
			if (attr == null)
			{
				return null;	//属性が見つからない
			}

			return m_compAttrValue.GetCompletionDataList(attr, info);
		}

		/// <summary>
		/// マクロ属性オブジェクトを取得する
		/// （再帰する）
		/// </summary>
		/// <param name="attrName">取得したい属性名</param>
		/// <param name="macro">属性の所属するマクロオブジェクト</param>
		/// <param name="macroList">マクロ情報リスト</param>
		/// <returns>属性オブジェクト</returns>
		private KagMacroAttr getMacroAttr(string attrName, KagMacro macro, KagMacro[] macroList)
		{
			if (overcheckReverseCount())
			{
				//再帰回数がオーバーしているときは何もせずに終了する
				return null;
			}

			//属性リストから検索する
			foreach (KagMacroAttr attr in macro.AttrTable.Values)
			{
				if (attrName == attr.Name)
				{
					return attr;	//属性を見つけた
				}
			}

			//省略属性リストから検索する
			KagMacro asterMacro = null;
			foreach (string tagName in macro.AsteriskTagList)
			{
				asterMacro = KagUtility.GetKagMacro(tagName, macroList);
				if (asterMacro == null)
				{
					continue;	//このマクロは見つからない
				}
				KagMacroAttr attr = getMacroAttr(attrName, asterMacro, macroList);
				if (attr != null)
				{
					return attr;	//属性を見つけた
				}
			}

			return null;	//見つからなかった
		}
		#endregion

		#region ループチェックメソッド
		/// <summary>
		/// 再帰回数をリセットする
		/// </summary>
		private void resetReverseCount()
		{
			m_reverseCount = 0;
		}

		/// <summary>
		/// 再帰回数をチェックしカウンタを増やす
		/// </summary>
		/// <returns>回数がオーバーしているときはtrue</returns>
		private bool overcheckReverseCount()
		{
			m_reverseCount++;
			if (m_reverseCount > MAX_LOOP_COUNT)
			{
				Debug.WriteLine("■overcheckReverseCount 最大再帰回数を超えました");
				return true;	//オーバーしている
			}
			else
			{
				return false;
			}
		}
		#endregion

		#region メソッド
		/// <summary>
		/// 入力補完時に置換する選択文字列をセットする
		/// </summary>
		/// <param name="textArea"></param>
		/// <param name="info"></param>
		/// <returns></returns>
		private string getCompReplaceText(ScintillaControl sci, KagTagKindInfo info)
		{
			string select = GetWordBeforeCaret(sci);
			if (select == null)
			{
				return null;
			}
			else if (select == "\"" && info.Kind == KagTagKindInfo.KagKind.AttrValue)
			{
				if (info.AttrValue != "")
				{
					select = "\"" + info.AttrValue + "\"";
				}
			}
			else if (select == "=" && info.Kind == KagTagKindInfo.KagKind.AttrValue)
			{
				if (info.AttrValue == "")
				{
					select = null;	//選択しない
				}
			}
			else if (select == "=\"" && info.Kind == KagTagKindInfo.KagKind.AttrValue)
			{
				if (info.AttrValue == "")
				{
					select = "\"";
				}
			}
			else if ((select == "[" || select == "@") && info.Kind == KagTagKindInfo.KagKind.KagTagName)
			{
				select = null;	//選択しない
			}
			else if ((select.EndsWith(" ") == true) && info.Kind == KagTagKindInfo.KagKind.AttrName)
			{
				select = null;	//選択しない
			}

			return select;
		}

		/// <summary>
		/// 現在のカーソル位置にあるワードを取得する
		/// </summary>
		/// <param name="textArea"></param>
		/// <returns></returns>
		public string GetWordBeforeCaret(ScintillaControl sci)
		{
			return sci.GetWordFromPosition(sci.CurrentPos);
		}
		#endregion
	}
}
