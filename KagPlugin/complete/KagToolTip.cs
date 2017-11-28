using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using KagContext.parse;

namespace KagContext.complete
{
	/// <summary>
	/// KAGツールチップ表示内容管理クラス
	/// </summary>
	public static class KagToolTip
	{
		/// <summary>
		/// ツールチップで表示するテキストを取得する
		/// </summary>
		/// <param name="document"></param>
		/// <param name="lineNumber"></param>
		/// <param name="colNumber"></param>
		/// <returns></returns>
		public static string GetText(ScintillaNet.ScintillaControl sci, int position)
		{
			if (sci == null)
			{
				return "";	//ドキュメントがないとき
			}

			string tip = "";
			string lineText = KagUtility.GetKagLineText(sci, position);
			string word = sci.GetWordFromPosition(position);
			KagTagKindInfo info = KagUtility.GetTagKind(lineText);
			if (info == null)
			{
				return "";	//取得できなかった
			}
			switch (info.Kind)
			{
				case KagTagKindInfo.KagKind.KagTagName:
					tip = getTagComment(word);
					break;
				case KagTagKindInfo.KagKind.AttrName:
					tip = getTagAttrComment(word, info);
					break;
				default:
					break;	//不明とか属性値は何もしない
			}

			return tip;
		}

		/// <summary>
		/// 指定したマクロ名の説明を取得する
		/// </summary>
		/// <param name="info"></param>
		/// <param name="macroName"></param>
		/// <returns></returns>
		private static string getTagComment(string macroName)
		{
			KagMacro macro = KagUtility.GetKagMacro(macroName);
			if (macro == null)
			{
				return "";
			}

			return macro.Comment;
		}

		/// <summary>
		/// 指定した属性の説明を取得する
		/// </summary>
		/// <param name="info"></param>
		/// <param name="attrName"></param>
		/// <returns></returns>
		private static string getTagAttrComment(string attrName, KagTagKindInfo info)
		{
			KagMacro[] macroList = PluginMain.ParserSrv.GetKagMacroList();
			if (macroList.Length == 0)
			{
				return "";	//一つもないとき
			}

			KagMacroAttr attr = KagUtility.GetKagMacroAttr(attrName, info, macroList);
			if (attr == null)
			{
				return "";	//属性が取得できなかったとき
			}

			return attr.Comment;
		}


	}
}
