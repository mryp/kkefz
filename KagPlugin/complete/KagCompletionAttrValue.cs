using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using KagContext.parse;
using KagContext.io;
using System.Drawing;
using PluginCore;

namespace KagContext.complete
{
	/// <summary>
	/// KAG属性値の入力補完リストを扱うクラス
	/// </summary>
	public class KagCompletionAttrValue
	{
		#region 定数
		//ファイル名リスト
		public const string TYPE_FILE_SCENARIO = "シナリオファイル名";
		public const string TYPE_FILE_IMAGE = "画像ファイル名";
		public const string TYPE_FILE_SE = "効果音ファイル名";
		public const string TYPE_FILE_CURSOR = "カーソルファイル名";
		public const string TYPE_FILE_BGM = "BGMファイル名";
		public const string TYPE_FILE_ACTION = "領域アクション定義ファイル名";
		public const string TYPE_FILE_PLUGIN = "プラグインファイル名";
		public const string TYPE_FILE_FONT = "フォントファイル名";
		public const string TYPE_FILE_VIDEO = "ムービーファイル名";

		//数値
		public const string TYPE_NUM_ZERO_OVER = "0以上の値";
		public const string TYPE_NUM_ONE_OVER = "1以上の値";
		public const string TYPE_NUM_PERCENT = "パーセント値";
		public const string TYPE_NUM_BYTE = "256値";
		public const string TYPE_NUM_MSTIME = "ミリ秒時間";
		public const string TYPE_NUM_REAL = "実数値";
		public const string TYPE_NUM_PAN = "-100～100の値";
		public const string TYPE_NUM_RGB = "RGB値";

		public const string TYPE_NUM_ARGB = "ARGB値";
		public const string TYPE_NUM_PMBYTE = "-255～255の値";
		public const string TYPE_NUM_HUE = "-180～180の値";

		//その他の文字列
		public const string TYPE_STRING_TJS = "TJS式";
		public const string TYPE_STRING_FONT = "フォント名";
		public const string TYPE_STRING_OTHER = "任意文字列";

		//最大値指定
		public const string TYPE_MAX_VIDEO_OBJECT = "ムービーオブジェクト番号";
		public const string TYPE_MAX_SE_BUFFER = "効果音バッファ番号";
		public const string TYPE_MAX_LAYER = "前景レイヤ";
		public const string TYPE_MAX_MESSAGE_LAYER = "メッセージレイヤ";

		//定数
		public const string TYPE_CONST_LAYER_PAGE = "レイヤーページ";
		public const string TYPE_CONST_LAYER_POS = "レイヤ位置";
		public const string TYPE_CONST_BOOL = "論理値";
		public const string TYPE_CONST_CURSOR = "カーソル定数";
		public const string TYPE_CONST_BASE_LAYER = "背景レイヤ";
		public const string TYPE_CONST_COLORCOMP_MODE = "合成モード";
		public const string TYPE_CONST_KAGEX_ACTION = "アクション";
		public const string TYPE_CONST_KAGEX_LTBMODE = "レイヤ切り替え種別";

		//プラグインによって変化
		public const string TYPE_OTHER_TRANSMETH = "トランジションタイプ";

		//シナリオ状態によって変化
		public const string TYPE_STATE_LABEL = "ラベル名";
		public const string TYPE_STATE_ASD_LABEL = "ASDラベル名";
		public const string TYPE_STATE_MACRO = "マクロ名";
        
		/// <summary>
		/// ファイル名を指定する属性名
		/// </summary>
		public const string TAG_ATTR_FILENAME = "storage";

		/// <summary>
		/// メッセージレイヤにつけるプレフィクス
		/// </summary>
		public const string TAG_PREFIX_MESSAGELAYER = "message";
		#endregion

        #region フィールド
        private Bitmap m_iconBitmap = null;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="icon"></param>
        public KagCompletionAttrValue(Bitmap icon)
        {
            m_iconBitmap = icon;
        }

		/// <summary>
		/// KAG属性値入力補完データリストを取得する
		/// </summary>
		/// <param name="attr">属性情報</param>
		/// <param name="compInfo">入力補完情報</param>
		/// <returns>入力補完データリスト</returns>
		public List<ICompletionListItem> GetCompletionDataList(KagMacroAttr attr, KagTagKindInfo compInfo)
		{
			if (attr == null)
			{
				return null;	//何も返せない
			}

			//設定取得
			string[] valueTypeList = splitAttrValue(attr.ValueType);
			if (valueTypeList == null)
			{
				return null;
			}

            List<ICompletionListItem> dataList = new List<ICompletionListItem>();
			foreach (string valueType in valueTypeList)
			{
				//属性値リスト取得
				string[] valueList = getAttrValueList(valueType, compInfo);
				if (valueList == null)
				{
					continue;
				}

				//取得した属性値を追加
				foreach (string value in valueList)
				{
                    string value2 = value;
                    if (PluginMain.EnvSettings.UseAttrValueDqRegion || value.IndexOf(' ') != -1)    //設定ONもしくは空白有の時
                    {
                        value2 = "\"" + value + "\"";
                    }
                    dataList.Add(new KagCompletionListItem(value, value2, attr.Comment, m_iconBitmap));
				}
			}

            return dataList;
		}

		/// <summary>
		/// 属性値タイプ文字列から属性値リストを取得する
		/// </summary>
		/// <param name="valueType">属性値タイプ</param>
		/// <param name="option">KAG入力補完オプション</param>
		/// <returns>属性値リスト</returns>
		private string[] getAttrValueList(string valueType, KagTagKindInfo compInfo)
		{
			string[] list = null;
			switch (valueType)
			{
				case TYPE_FILE_SCENARIO:
					list = getFileNameList(PluginMain.EnvSettings.ScenarioFileExt, TYPE_FILE_SCENARIO);
					break;
				case TYPE_FILE_IMAGE:
                    list = getFileNameList(PluginMain.EnvSettings.ImageFileExt, TYPE_FILE_IMAGE);
					break;
				case TYPE_FILE_SE:
                    list = getFileNameList(PluginMain.EnvSettings.SeFileExt, TYPE_FILE_SE);
					break;
				case TYPE_FILE_CURSOR:
                    list = getFileNameList(PluginMain.EnvSettings.CursorFileExt, TYPE_FILE_CURSOR);
					break;
				case TYPE_FILE_BGM:
                    list = getFileNameList(PluginMain.EnvSettings.BgmFileExt, TYPE_FILE_BGM);
					break;
				case TYPE_FILE_ACTION:
                    list = getFileNameList(PluginMain.EnvSettings.ActionFileExt, TYPE_FILE_ACTION);
					break;
				case TYPE_FILE_PLUGIN:
                    list = getFileNameListPlugin(PluginMain.EnvSettings.PluginFileExt, TYPE_FILE_PLUGIN);
					break;
				case TYPE_FILE_FONT:
                    list = getFileNameList(PluginMain.EnvSettings.FontFileExt, TYPE_FILE_FONT);
					break;
				case TYPE_FILE_VIDEO:
                    list = getFileNameList(PluginMain.EnvSettings.VideoFileExt, TYPE_FILE_VIDEO);
					break;
				case TYPE_NUM_ZERO_OVER:
                    list = splitAttrValue(PluginMain.EnvSettings.ZeroOverNumberList);
					break;
				case TYPE_NUM_ONE_OVER:
                    list = splitAttrValue(PluginMain.EnvSettings.OneOverNumberList);
					break;
				case TYPE_NUM_PERCENT:
                    list = splitAttrValue(PluginMain.EnvSettings.PercentNumberList);
					break;
				case TYPE_NUM_BYTE:
                    list = splitAttrValue(PluginMain.EnvSettings.ByteNumberList);
					break;
				case TYPE_NUM_MSTIME:
                    list = splitAttrValue(PluginMain.EnvSettings.MsTimeNumberList);
					break;
				case TYPE_NUM_REAL:
                    list = splitAttrValue(PluginMain.EnvSettings.RealNumberList);
					break;
				case TYPE_NUM_PAN:
                    list = splitAttrValue(PluginMain.EnvSettings.PmHundredNumberList);
					break;
				case TYPE_NUM_RGB:
                    list = splitAttrValue(PluginMain.EnvSettings.RgbNumberList);
					break;
				case TYPE_NUM_ARGB:
                    list = splitAttrValue(PluginMain.EnvSettings.ArgbNumberList);
					break;
				case TYPE_NUM_PMBYTE:
                    list = splitAttrValue(PluginMain.EnvSettings.PmbyteNumberList);
					break;
				case TYPE_NUM_HUE:
                    list = splitAttrValue(PluginMain.EnvSettings.HueNumberList);
					break;
				case TYPE_STRING_TJS:
                    list = splitAttrValue(PluginMain.EnvSettings.TjsStringList);
					break;
				case TYPE_STRING_FONT:
                    list = splitAttrValue(PluginMain.EnvSettings.FontStringList);
					break;
				case TYPE_STRING_OTHER:
                    list = splitAttrValue(PluginMain.EnvSettings.OtherStringList);
					break;
				case TYPE_MAX_VIDEO_OBJECT:
                    list = getNumberList(0, PluginMain.EnvSettings.VideoBufferMaxNumber - 1);
					break;
				case TYPE_MAX_SE_BUFFER:
                    list = getNumberList(0, PluginMain.EnvSettings.SeBufferMaxNumber - 1);
					break;
				case TYPE_MAX_LAYER:
                    list = getNumberList(0, PluginMain.EnvSettings.LayerMaxNumber - 1);
					break;
				case TYPE_MAX_MESSAGE_LAYER:
                    list = getNumberListForMeslay(0, PluginMain.EnvSettings.MessageLayerMaxNumber - 1);
					break;
				case TYPE_CONST_LAYER_PAGE:
                    list = splitAttrValue(PluginMain.EnvSettings.LayerPageList);
					break;
				case TYPE_CONST_LAYER_POS:
                    list = splitAttrValue(PluginMain.EnvSettings.LayerPosList);
					break;
				case TYPE_CONST_BOOL:
                    list = splitAttrValue(PluginMain.EnvSettings.BoolValueList);
					break;
				case TYPE_CONST_CURSOR:
                    list = splitAttrValue(PluginMain.EnvSettings.CursorDefList);
					break;
				case TYPE_CONST_BASE_LAYER:
                    list = splitAttrValue(PluginMain.EnvSettings.BaseLayerList);
					break;
				case TYPE_OTHER_TRANSMETH:
					list = getTransMthodList();
					break;
				case TYPE_STATE_LABEL:
					list = getLabelList(compInfo);
					break;
				case TYPE_STATE_ASD_LABEL:
					//未実装
					break;
				case TYPE_CONST_COLORCOMP_MODE:
                    list = splitAttrValue(PluginMain.EnvSettings.ColorcompModeList);
					break;
				case TYPE_STATE_MACRO:
					list = getMacroNameList();
					break;
				default:
					list = new string[] {valueType};	//見つからないときはそのままと判断する
					break;
			}

			return list;
		}

		/// <summary>
		/// 属性値タイプリストを分解する
		/// （例：0;1;2）
		/// </summary>
		/// <param name="valueList">属性値リスト</param>
        /// <returns>属性タイプ</returns>
        private static string[] splitAttrValue(string valueList)
        {
            string[] result = valueList.Split(';');
            return result;
        }

		private static string[] splitAttrValue(List<string> valueList)
		{
            return valueList.ToArray();
		}

		/// <summary>
		/// 数値リストを作成する
		/// </summary>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		/// <returns>最小値から最大値までの数値リスト</returns>
		private static string[] getNumberList(int min, int max)
		{
			List<string> list = new List<string>();
			for (int i = min; i <= max; i++)
			{
				list.Add(i.ToString());
			}

			return list.ToArray();
		}

		/// <summary>
		/// メッセージレイヤリストを作成する
		/// </summary>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		/// <returns>最小値から最大値までのメッセージレイヤリスト</returns>
		private static string[] getNumberListForMeslay(int min, int max)
		{
			string[] numList = getNumberList(min, max);

			List<string> list = new List<string>();
			list.Add(TAG_PREFIX_MESSAGELAYER);		//デフォルトメッセージレイヤ
			foreach (string num in numList)
			{
				list.Add(TAG_PREFIX_MESSAGELAYER + num);
			}

			return list.ToArray();
		}

		/// <summary>
		/// ファイル名リストを取得する
		/// </summary>
		/// <param name="pattern">検索拡張子パターン（例：*.png;*.jpg）</param>
		/// <param name="valueType">属性値タイプ</param>
		/// <returns>ファイル名リスト</returns>
        private static string[] getFileNameList(List<string> pattern, string valueType)
		{
			//検索ディレクトリを取得する
			string dirPath = EnvPath.Instance.ProjectKagDataDirPath;
			if (String.IsNullOrEmpty(dirPath))
			{
				return null;
			}

			//パスを取得する
			string[] pathList = FileUtil.GetDirectoryFile(dirPath, pattern, SearchOption.AllDirectories);
			List<string> fileNameList = new List<string>();
			foreach (string path in pathList)
			{
				//ファイル名に変換する
				switch (valueType)
				{
					case TYPE_FILE_IMAGE:
					case TYPE_FILE_SE:
					case TYPE_FILE_BGM:
						fileNameList.Add(Path.GetFileNameWithoutExtension(path));	//拡張子なし
						break;
					default:
						fileNameList.Add(Path.GetFileName(path));					//拡張子あり
						break;
				}
			}

			return fileNameList.ToArray();
		}

		/// <summary>
		/// ファイル名リストを取得する（プラグイン専用）
		/// </summary>
		/// <param name="pattern">検索パターン</param>
		/// <param name="valueType">プラグインを表す値</param>
		/// <returns>取得したファイル名リスト</returns>
		private static string[] getFileNameListPlugin(List<string> pattern, string valueType)
		{
			//検索ディレクトリを取得する
            string exeDirPath = EnvPath.Instance.ProjectDirPath;		//実行フォルダと同じパス
			if (String.IsNullOrEmpty(exeDirPath))
			{
				return null;
			}
			exeDirPath = Path.GetDirectoryName(exeDirPath);
			string pluginDirPath = Path.Combine(exeDirPath, "plugin");	//プラグインフォルダ
			string[] dirList = { exeDirPath, pluginDirPath };

			//パスを取得する
			List<string> fileNameList = new List<string>();
			foreach (string dirPath in dirList)
			{
				string[] pathList = FileUtil.GetDirectoryFile(dirPath, pattern, SearchOption.TopDirectoryOnly);
				foreach (string path in pathList)
				{
					//ファイル名に変換する
					fileNameList.Add(Path.GetFileName(path));	//プラグインは必ず拡張子あり
				}
			}

			return fileNameList.ToArray();
		}

		/// <summary>
		/// ラベル名リストを取得する
		/// </summary>
		/// <returns>ラベル名リスト</returns>
		private static string[] getLabelList(KagTagKindInfo compInfo)
		{
			string fileName = "";
			if (compInfo.AttrTable.ContainsKey(TAG_ATTR_FILENAME)
			&& compInfo.AttrTable[TAG_ATTR_FILENAME] != "")
			{
				//ファイル名がすでに指定され散るときはそのファイルを探す
				fileName = compInfo.AttrTable[TAG_ATTR_FILENAME];
			}
			else
			{
				//セットされていないときは現在のファイルのラベルを表示する
                fileName = Path.GetFileName(PluginCore.PluginBase.MainForm.CurrentDocument.FileName);	//現在開いているファイル名
			}

			List<string> list = new List<string>();
			foreach (KagLabelItem item in PluginMain.ParserSrv.GetLabelListAll())
			{
				if (Path.GetFileName(item.FilePath) == fileName)	//指定したファイルのみ登録する
				{
					list.Add(item.LabelName);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// マクロ名リストを取得する
		/// </summary>
		/// <returns>マクロ名リスト</returns>
		private static string[] getMacroNameList()
		{
			List<string> list = new List<string>();
			foreach (KagMacro macro in PluginMain.ParserSrv.GetKagMacroList())
			{
				list.Add(macro.Name);
			}

			return list.ToArray();
		}

		/// <summary>
		/// トランジションメソッド名リストを取得する
		/// （暫定対応）
		/// </summary>
		/// <returns>トランジションメソッド名リスト</returns>
		private static string[] getTransMthodList()
		{
			List<string> list = new List<string>();

			//デフォルト
			list.Add("crossfade");
			list.Add("universal");
			list.Add("scroll");

			//extrans.dll使用時
			list.Add("wave");
			list.Add("mosaic");
			list.Add("turn");
			list.Add("rotatezoom");
			list.Add("rotatevanish");
			list.Add("rotateswap");
			list.Add("ripple");

			return list.ToArray();
		}
	}
}
