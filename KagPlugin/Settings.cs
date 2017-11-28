using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace KagContext
{
	[Serializable]
	public class Settings
	{
		#region フィールド
		//全体
		private bool m_useAttrValueDqRegion = false;
		private List<String> m_convertFileExt = new List<String> { ".ks", ".tjs" };

		//ファイル名拡張子
		private List<String> m_scenarioFileExt = new List<String> { "*.ks" };
		private List<String> m_imageFileExt = new List<String> { "*.bmp", "*.jpg", "*.jpeg", "*.jpe", "*.png", "*.eri", "*.tlg" };
		private List<String> m_seFileExt = new List<String> { "*.wav", "*.ogg", "*.tc" };
		private List<String> m_cursorFileExt = new List<String> { "*.cur", "*.ani" };
		private List<String> m_bgmFileExt = new List<String> { "*.wav", "*.ogg", "*.tcw", "*.smf", "*.mid" };
		private List<String> m_actionFileExt = new List<String> { "*.ma" };
		private List<String> m_pluginFileExt = new List<String> { "*.dll" };
		private List<String> m_fontFileExt = new List<String> { "*.tft" };
		private List<String> m_videoFileExt = new List<String> { "*.avi", "*.mpg", "*.mpeg", "*.mpv", "*.swf" };

		//数値
		private List<String> m_zeroOverNumberList = new List<String> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100", "200", "500", "1000" };
		private List<String> m_oneOverNumberList = new List<String> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100", "200", "500", "1000" };
		private List<String> m_percentNumberList = new List<String> { "0", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100" };
		private List<String> m_byteNumberList = new List<String> { "0", "16", "32", "48", "64", "80", "96", "112", "128", "144", "160", "176", "192", "208", "224", "240", "255" };
		private List<String> m_msTimeNumberList = new List<String> { "0", "100", "200", "300", "400", "500", "600", "700", "800", "900", "1000", "1200", "1500", "2000", "3000", "5000" };
		private List<String> m_realNumberList = new List<String> { "-10.0", "-5.0", "-2.0", "-1.0", "-0.5", "-0.4", "-0.3", "-0.2", "-0.1", "-0.8", "0", "0.1", "0.2", "0.3", "0.4", "0.5", "0.8", "1.0", "2.0", "5.0", "10.0" };
		private List<String> m_pmHundredNumberList = new List<String> { "-100", "-90", "-80", "-70", "-60", "-50", "-40", "-30", "-20", "-10", "0", "10", "20", "30", "40", "50", "60", "70", "80", "90", "100" };
		private List<String> m_rgbNumberList = new List<String> { "0x000000", "0xFFFFFF" };
		private List<String> m_argbNumberList = new List<String> { "0x00000000", "0x00FFFFFF", "0xFF000000", "0xFFFFFFFF" };
		private List<String> m_pmbyteNumberList = new List<String> { "0", "16", "32", "48", "64", "80", "96", "112", "128", "144", "160", "176", "192", "208", "224", "240", "255", "-16", "-32", "-48", "-64", "-80", "-96", "-112", "-128", "-144", "-160", "-176", "-192", "-208", "-224", "-240", "-255" };
		private List<String> m_hueNumberList = new List<String> { "0", "30", "60", "90", "120", "150", "180", "-30", "-60", "-90", "-120", "-150", "-180" };

		//任意文字列
		private List<String> m_otherStringList = new List<String> { "" };
		private List<String> m_tjsStringList = new List<String> { "" };
		private List<String> m_fontStringList = new List<String> { "ＭＳ ゴシック", "ＭＳ 明朝", "ＭＳ Ｐゴシック", "ＭＳ Ｐ明朝" };

		//最大値指定
		private int m_layerMaxNumber = 3;
		private int m_messageLayerMaxNumber = 2;
		private int m_seBufferMaxNumber = 3;
		private int m_videoBufferMaxNumber = 3;

		//定数
		private List<String> m_baseLayerList = new List<String> { "base" };
		private List<String> m_boolValueList = new List<String> { "true", "false" };
		private List<String> m_layerPageList = new List<String> { "fore", "back" };
		private List<String> m_layerPosList = new List<String> { "left", "left_center", "center", "right_center", "right" };
		private List<String> m_cursorDefList = new List<String> { "crDefault", "crNone", "crArrow", "crCross", "crIBeam", "crHBeam", "crSizeNESW", "crSizeNS", "crSizeNWSE", "crSizeWE", "crUpArrow", "crHourGlass", "crDrag", "crNoDrop", "crHSplit", "crVSplit", "crMultiDrag", "crSQLWait", "crNo", "crAppStart", "crHelp", "crHandPoint", "crSizeAll" };
		private List<String> m_colorcompModeList = new List<String> { "ltOpaque", "ltAlpha", "ltAddAlpha", "ltAdditive", "ltSubtractive", "ltMultiplicative", "ltDodge", "ltLighten", "ltDarken", "ltScreen", "ltPsNormal", "ltPsAdditive", "ltPsSubtractive", "ltPsMultiplicative", "ltPsScreen", "ltPsOverlay", "ltPsHardLight", "ltPsSoftLight", "ltPsColorDodge", "ltPsColorDodge5", "ltPsColorBurn", "ltPsLighten", "ltPsDarken", "ltPsDifference", "ltPsDifference5", "ltPsExclusion" };
		#endregion

		#region プロパティ
		/// <summary>
		/// 属性値入力時に"で囲むかどうか
		/// </summary>
		[Description("属性値入力時に\"で囲むかどうか"),
		 DefaultValue(false)]
		[Category("全般")]
		public bool UseAttrValueDqRegion
		{
			get { return m_useAttrValueDqRegion; }
			set { m_useAttrValueDqRegion = value; }
		}

		/// <summary>
		/// シナリオファイルの拡張子リスト
		/// </summary>
		[Description("シナリオファイルの拡張子リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("ファイル名拡張子")]
		public List<String> ScenarioFileExt
		{
			get { return m_scenarioFileExt; }
			set { m_scenarioFileExt = value; }
		}

		/// <summary>
		/// 画像ファイルの拡張子リスト
		/// </summary>
		[Description("画像ファイルの拡張子リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("ファイル名拡張子")]
		public List<String> ImageFileExt
		{
			get { return m_imageFileExt; }
			set { m_imageFileExt = value; }
		}

		/// <summary>
		/// 効果音ファイルの拡張子リスト
		/// </summary>
		[Description("効果音ファイルの拡張子リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("ファイル名拡張子")]
		public List<String> SeFileExt
		{
			get { return m_seFileExt; }
			set { m_seFileExt = value; }
		}

		/// <summary>
		/// カーソルファイルの拡張子リスト
		/// </summary>
		[Description("カーソルファイルの拡張子リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("ファイル名拡張子")]
		public List<String> CursorFileExt
		{
			get { return m_cursorFileExt; }
			set { m_cursorFileExt = value; }
		}

		/// <summary>
		/// BGMファイルの拡張子リスト
		/// </summary>
		[Description("BGMファイルの拡張子リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("ファイル名拡張子")]
		public List<String> BgmFileExt
		{
			get { return m_bgmFileExt; }
			set { m_bgmFileExt = value; }
		}

		/// <summary>
		/// 領域アクション定義ファイルの拡張子リスト
		/// </summary>
		[Description("領域アクション定義ファイルの拡張子リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("ファイル名拡張子")]
		public List<String> ActionFileExt
		{
			get { return m_actionFileExt; }
			set { m_actionFileExt = value; }
		}

		/// <summary>
		/// プラグインファイルの拡張子リスト
		/// </summary>
		[Description("プラグインファイルの拡張子リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("ファイル名拡張子")]
		public List<String> PluginFileExt
		{
			get { return m_pluginFileExt; }
			set { m_pluginFileExt = value; }
		}

		/// <summary>
		/// レンダリング済みフォントファイルの拡張子リスト
		/// </summary>
		[Description("レンダリング済みフォントファイルの拡張子リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("ファイル名拡張子")]
		public List<String> FontFileExt
		{
			get { return m_fontFileExt; }
			set { m_fontFileExt = value; }
		}

		/// <summary>
		/// ムービーファイルの拡張子リスト
		/// </summary>
		[Description("ムービーファイルの拡張子リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("ファイル名拡張子")]
		public List<String> VideoFileExt
		{
			get { return m_videoFileExt; }
			set { m_videoFileExt = value; }
		}

		/// <summary>
		/// 0以上の数値リスト（セミコロン区切り）
		/// </summary>
		[Description("0以上の数値リスト（セミコロン区切り）")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> ZeroOverNumberList
		{
			get { return m_zeroOverNumberList; }
			set { m_zeroOverNumberList = value; }
		}

		/// <summary>
		/// 1以上の数値リスト（セミコロン区切り）
		/// </summary>
		[Description("1以上の数値リスト（セミコロン区切り）")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> OneOverNumberList
		{
			get { return m_oneOverNumberList; }
			set { m_oneOverNumberList = value; }
		}

		/// <summary>
		/// 0～100の数値リスト（セミコロン区切り）
		/// </summary>
		[Description("0～100の数値リスト（セミコロン区切り）")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> PercentNumberList
		{
			get { return m_percentNumberList; }
			set { m_percentNumberList = value; }
		}

		/// <summary>
		/// 0～255の数値リスト（セミコロン区切り）
		/// </summary>
		[Description("0～255の数値リスト（セミコロン区切り）")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> ByteNumberList
		{
			get { return m_byteNumberList; }
			set { m_byteNumberList = value; }
		}

		/// <summary>
		/// ミリ秒時間の数値リスト（セミコロン区切り）
		/// </summary>
		[Description("ミリ秒時間の数値リスト（セミコロン区切り）")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> MsTimeNumberList
		{
			get { return m_msTimeNumberList; }
			set { m_msTimeNumberList = value; }
		}

		/// <summary>
		/// 実数の数値リスト（セミコロン区切り）
		/// </summary>
		[Description("実数の数値リスト（セミコロン区切り）")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> RealNumberList
		{
			get { return m_realNumberList; }
			set { m_realNumberList = value; }
		}

		/// <summary>
		/// -100～100の数値リスト（セミコロン区切り）
		/// </summary>
		[Description("-100～100の数値リスト（セミコロン区切り）")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> PmHundredNumberList
		{
			get { return m_pmHundredNumberList; }
			set { m_pmHundredNumberList = value; }
		}

		/// <summary>
		/// 0xRRGGBB形式の数値（セミコロン区切り）
		/// </summary>
		[Description("0xRRGGBB形式の数値（セミコロン区切り）")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> RgbNumberList
		{
			get { return m_rgbNumberList; }
			set { m_rgbNumberList = value; }
		}

		/// <summary>
		/// 0xAARRGGBB形式の数値（セミコロン区切り）
		/// </summary>
		[Description("0xAARRGGBB形式の数値（セミコロン区切り）")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> ArgbNumberList
		{
			get { return m_argbNumberList; }
			set { m_argbNumberList = value; }
		}

		/// <summary>
		/// -255～255の数値リスト
		/// </summary>
		[Description("-255～255の数値リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> PmbyteNumberList
		{
			get { return m_pmbyteNumberList; }
			set { m_pmbyteNumberList = value; }
		}

		/// <summary>
		/// -180～180の数値リスト
		/// </summary>
		[Description("-180～180の数値リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("数値")]
		public List<String> HueNumberList
		{
			get { return m_hueNumberList; }
			set { m_hueNumberList = value; }
		}

		/// <summary>
		/// 任意文字列リスト
		/// </summary>
		[Description("任意文字列リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("文字列")]
		public List<String> OtherStringList
		{
			get { return m_otherStringList; }
			set { m_otherStringList = value; }
		}

		/// <summary>
		/// TJS式文字列リスト
		/// </summary>
		[Description("TJS式文字列リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("文字列")]
		public List<String> TjsStringList
		{
			get { return m_tjsStringList; }
			set { m_tjsStringList = value; }
		}

		/// <summary>
		/// フォント名リスト
		/// </summary>
		[Description("フォント名リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("文字列")]
		public List<String> FontStringList
		{
			get { return m_fontStringList; }
			set { m_fontStringList = value; }
		}

		/// <summary>
		/// 前景レイヤ数の最大値
		/// </summary>
		[Description("前景レイヤ数の最大値"),
		 DefaultValue(3)]
		[Category("最大値")]
		public int LayerMaxNumber
		{
			get { return m_layerMaxNumber; }
			set { m_layerMaxNumber = value; }
		}

		/// <summary>
		/// メッセージレイヤ数の最大値
		/// </summary>
		[Description("メッセージレイヤ数の最大値"),
		 DefaultValue(2)]
		[Category("最大値")]
		public int MessageLayerMaxNumber
		{
			get { return m_messageLayerMaxNumber; }
			set { m_messageLayerMaxNumber = value; }
		}

		/// <summary>
		/// 効果音バッファ数の最大値
		/// </summary>
		[Description("効果音バッファ数の最大値"),
		 DefaultValue(3)]
		[Category("最大値")]
		public int SeBufferMaxNumber
		{
			get { return m_seBufferMaxNumber; }
			set { m_seBufferMaxNumber = value; }
		}

		/// <summary>
		/// ムービーオブジェクト番号の最大値
		/// </summary>
		[Description("ムービーオブジェクト番号の最大値"),
		 DefaultValue(3)]
		[Category("最大値")]
		public int VideoBufferMaxNumber
		{
			get { return m_videoBufferMaxNumber; }
			set { m_videoBufferMaxNumber = value; }
		}

		/// <summary>
		/// 背景レイヤリスト
		/// </summary>
		[Description("背景レイヤリスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("その他定数")]
		public List<String> BaseLayerList
		{
			get { return m_baseLayerList; }
			set { m_baseLayerList = value; }
		}

		/// <summary>
		/// 論理値リスト
		/// </summary>
		[Description("論理値リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("その他定数")]
		public List<String> BoolValueList
		{
			get { return m_boolValueList; }
			set { m_boolValueList = value; }
		}

		/// <summary>
		/// レイヤーページリスト
		/// </summary>
		[Description("レイヤーページリスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("その他定数")]
		public List<String> LayerPageList
		{
			get { return m_layerPageList; }
			set { m_layerPageList = value; }
		}

		/// <summary>
		/// レイヤー位置リスト
		/// </summary>
		[Description("レイヤー位置リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("その他定数")]
		public List<String> LayerPosList
		{
			get { return m_layerPosList; }
			set { m_layerPosList = value; }
		}

		/// <summary>
		/// カーソル定数リスト
		/// </summary>
		[Description("カーソル定数リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("その他定数")]
		public List<String> CursorDefList
		{
			get { return m_cursorDefList; }
			set { m_cursorDefList = value; }
		}

		/// <summary>
		/// 合成モード定数リスト
		/// </summary>
		[Description("合成モード定数リスト")]
		[Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor,System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[Category("その他定数")]
		public List<String> ColorcompModeList
		{
			get { return m_colorcompModeList; }
			set { m_colorcompModeList = value; }
		}

		#endregion
	}
}
