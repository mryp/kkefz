using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using ScintillaNet;
using System.Diagnostics;
using System.IO;
using KagContext.parse;

namespace KagContext
{
    /// <summary>
    /// KAGカラー設定用Lexer
    /// </summary>
    public class KagStyleLexer
    {
        #region 定数
        //専用スタイル
        private const int STYLE_DEFAULT = (int)ScintillaNet.Lexers.XML.DEFAULT;
        private const int STYLE_COMMENT = (int)ScintillaNet.Lexers.XML.COMMENT;
        private const int STYLE_LABEL = (int)ScintillaNet.Lexers.XML.CDATA;
        private const int STYLE_TAG = (int)ScintillaNet.Lexers.XML.TAG;
        private const int STYLE_TAG_ATTR = (int)ScintillaNet.Lexers.XML.ATTRIBUTE;
        private const int STYLE_TAG_ATTR_VALUE = (int)ScintillaNet.Lexers.XML.VALUE;
        private const int STYLE_TJS_SCRIPT = (int)ScintillaNet.Lexers.XML.SCRIPT;
        
        //エンコード
        private Encoding SJIS = Encoding.GetEncoding(932);

        public const uint SC_FOLDLEVELBASE = 0x400;
        public const uint SC_FOLDLEVELWHITEFLAG = 0x1000;
        public const uint SC_FOLDLEVELHEADERFLAG = 0x2000;
        public const uint SC_FOLDLEVELBOXHEADERFLAG = 0x4000;
        public const uint SC_FOLDLEVELBOXFOOTERFLAG = 0x8000;
        public const uint SC_FOLDLEVELCONTRACTED = 0x10000;
        public const uint SC_FOLDLEVELUNINDENT = 0x20000;
        public const uint SC_FOLDLEVELNUMBERMASK = 0x0FFF;

        public const int FOLDING_NONE = 0;
        public const int FOLDING_LABEL = 1;
        public const int FOLDING_REGION = 2;
        #endregion

        #region フィールド
        /// <summary>スタイル設定を行ったかどうか（true=未設定、false=設定済み）</summary>
        private bool m_isFirstStyleSetting = true;
        private int m_col = 0;
        private int m_line = 0;
        private int m_lineHeadTabNum = 0;
        private int m_index = 0;
        private int m_startPos = 0;
        private int m_foldingLevel = 0;
        private ScintillaControl m_sci;
        private TextReader m_reader;
        #endregion

        #region プロパティ
        /// <summary>
        /// 現在開いているカレントファイル
        /// </summary>
        public string CurrentFile 
        {
            get;
            set; 
        }
        #endregion

        #region 初期化・終了処理メソッド
        /// <summary>
        /// 初期設定
        /// </summary>
        /// <param name="scintilla"></param>
        public void Init(ScintillaControl scintilla)
        {
            if (scintilla == null || scintilla.ConfigurationLanguage != "kag")
            {
                return;
            }
            
            Debug.WriteLine("KagStyleLexer#Init codePage=" + scintilla.CodePage.ToString());
            m_isFirstStyleSetting = true;
            
            //KAG用エディタに設定を上書き
            scintilla.Lexer = 0;
            scintilla.StyleNeeded -= scintilla_StyleNeeded;
            scintilla.StyleNeeded += new StyleNeededHandler(scintilla_StyleNeeded);
            scintilla.WrapStartIndent = 0;  //折り返しインデントは強制的にOFFとする

            initStyleSetting(scintilla);
            preStartStyle(scintilla);
        }

        /// <summary>
        /// スタイルの再更新を行う
        /// </summary>
        /// <param name="scintilla"></param>
        public void RefreshStyle(ScintillaControl scintilla)
        {
            if (scintilla == null || scintilla.ConfigurationLanguage != "kag")
            {
                return;
            }

            scintilla.Lexer = 0;
            preStartStyle(scintilla);
        }

        /// <summary>
        /// スタイル情報の初期設定
        /// スタイルセットの直前でセットすること
        /// </summary>
        /// <param name="sci"></param>
        private void initStyleSetting(ScintillaControl sci)
        {
            if (m_isFirstStyleSetting)
            {
                m_isFirstStyleSetting = false;

                //カラー設定
                /*
                sci.StyleClearAll();
                sci.StyleResetDefault();
                */
                /*
                string defaultForeColor = "0xDDDDDD";
                string defaultBackColor = "0x222222";
                string defaultFontName = "MS GOTHIC";
                int defaultFontSize = 10;

                setColor(sci, STYLE_COMMENT, "0x64B1FF", defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_LABEL, "0x80FF00", defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_TAG, "0xFF8080", defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_TAG_ATTR, "0xC0C040", defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_TAG_ATTR_VALUE, "0x00A45F", defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_TJS_SCRIPT, "0xDDDDDD", defaultBackColor, defaultFontName, defaultFontSize);
                */
                /*
                //デフォルトスタイル
                setColor(sci, STYLE_DEFAULT, defaultForeColor, defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_GDEFAULT, defaultForeColor, defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_LINENUMBER, "0x222222", "0xDDDDDD", defaultFontName, defaultFontSize);
                setColor(sci, STYLE_BRACELIGHT, "0x0000cc", "0xcdcdff", defaultFontName, defaultFontSize);
                setColor(sci, STYLE_BRACEBAD, defaultForeColor, defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_CONTROLCHAR, defaultForeColor, defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_INDENTGUIDE, defaultForeColor, defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_CALLTIP, defaultForeColor, defaultBackColor, defaultFontName, defaultFontSize);
                setColor(sci, STYLE_LASTPREDEFINED, defaultForeColor, defaultBackColor, defaultFontName, defaultFontSize);
                */
            }
            else
            {
                //2回目以降のみ
                PluginMain.ParserSrv.ParseFile(this.CurrentFile, m_sci.Text);
            }
        }

        /// <summary>
        /// カラー設定情報を設定する
        /// </summary>
        /// <param name="sci"></param>
        /// <param name="style"></param>
        /// <param name="foreColor"></param>
        /// <param name="backColor"></param>
        /// <param name="fontName"></param>
        /// <param name="fontSize"></param>
        private void setColor(ScintillaControl sci, int style, string foreColor, string backColor, string fontName, int fontSize)
        {
            sci.StyleSetFore(style, resolveColor(foreColor));
            sci.StyleSetBack(style, resolveColor(backColor));
            sci.StyleSetFont(style, fontName);
            sci.StyleSetSize(style, fontSize);
        }

        /// <summary>
        /// 文字列表示からカラーを取得する
        /// </summary>
        /// <param name="aColor">0xFFFFFF 形式のカラーを表す16進数文字列</param>
        /// <returns>カラー整数値</returns>
        private int resolveColor(string aColor)
        {
            return TO_COLORREF(Int32.Parse(aColor.Substring(2), System.Globalization.NumberStyles.HexNumber));
        }

        /// <summary>
        /// RとBを逆転させる
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private int TO_COLORREF(int c)
        {
            return (((c & 0xff0000) >> 16) + ((c & 0x0000ff) << 16) + (c & 0x00ff00));
        }
        #endregion

        #region スタイル設定
        /// <summary>
        /// スタイル変更通知イベント
        /// </summary>
        /// <param name="sci">エディタ</param>
        /// <param name="position">変更位置</param>
        void scintilla_StyleNeeded(ScintillaControl sci, int position)
        {
            initStyleSetting(sci);
            preStartStyle(sci);
        }

        /// <summary>
        /// スタイルの設定前にフィールドの初期化を行う
        /// </summary>
        /// <param name="sci"></param>
        private void preStartStyle(ScintillaControl sci)
        {
            m_sci = sci;
            m_index = 0;
            m_lineHeadTabNum = 0;
            m_line = 0;
            m_col = 0;
            m_startPos = 0;
            m_foldingLevel = FOLDING_NONE;
            m_reader = new StringReader(m_sci.Text);
            startStyle();
        }

        /// <summary>
        /// 現在の位置を読み込み次へ移動する
        /// </summary>
        /// <returns></returns>
        private int readerRead()
        {
            int current = m_reader.Read();
            m_index += m_sci.MBSafeTextLength(((char)current).ToString());
            m_col++;
            return current;
        }

        /// <summary>
        /// 位置を移動せずに次の値を返す
        /// </summary>
        /// <returns></returns>
        private int readerPeek()
        {
            return m_reader.Peek();
        }
        
        /// <summary>
        /// 行末かどうかを返す
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool wasLineEnd(char ch)
        {
            // Handle MS-DOS or MacOS line ends.
            if (ch == '\r')
            {
                if (m_reader.Peek() == '\n')
                { // MS-DOS line end '\r\n'
                    ch = (char)readerRead();
                }
                else
                { // assume MacOS line end which is '\r'
                    ch = '\n';
                }
            }

            return ch == '\n';
        }

        /// <summary>
        /// 行末かどうかチェックして返す
        /// 行末だったときは進める
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool handleLineEnd(char ch)
        {
            if (wasLineEnd(ch))
            {
                m_line++;
                m_col = 0;
                m_lineHeadTabNum = 0;
                m_sci.SetFoldLevel(m_line, (int)(SC_FOLDLEVELBASE + m_foldingLevel));
                return true;
            }
            return false;
        }

        /// <summary>
        /// 行末までスキップする
        /// </summary>
        private void skipToEndOfLine()
        {
            int nextChar;
            while ((nextChar = readerRead()) != -1)
            {
                if (handleLineEnd((char)nextChar))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 行末までスキップしテキストを取得する
        /// </summary>
        /// <returns></returns>
        private string skipToEndOfLineText()
        {
            StringBuilder sb = new StringBuilder(128);
            int nextChar;
            while ((nextChar = readerRead()) != -1)
            {
                if (handleLineEnd((char)nextChar))
                {
                    break;
                }
                sb.Append((char)nextChar);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 現在の位置が行頭かどうか
        /// </summary>
        /// <param name="c"></param>
        /// <returns>現在位置が行頭の時true</returns>
        protected bool IsKagLineHead(char c)
        {
            if (m_col == 1)
            {
                return true;	//行頭
            }
            if (m_col == m_lineHeadTabNum + 1)
            {
                return true;	//先頭にはタブしかない
            }

            return false;	//行頭ではない
        }

        /// <summary>
        /// スタイル設定を開始する
        /// </summary>
        private void startStyle()
        {
            Debug.WriteLine("KagStyleLexer EndStyled=" + m_sci.EndStyled + " firstStyle=" + m_sci.StyleAt(0));
            m_sci.StartStyling(0, 0x1f);
            
            int nextChar;
            char c;
            while ((nextChar = this.readerRead()) != -1)
            {
                c = (char)nextChar;
                bool isSetStyle = false;
                switch (c)
                {
                    case ' ':		//空白
                        break;
                    case '\t':
                        m_lineHeadTabNum++;	//タブの数を増やす
                        break;
                    case '\r':		//改行
                    case '\n':
                        handleLineEnd(c);
                        break;
                    case ';':		//コメント
                        if (IsKagLineHead(c) == false)
                        {
                            break;	//コメントではない
                        }

                        skipToEndOfLine();  //最後に移動
                        setStyleCh(STYLE_COMMENT);
                        isSetStyle = true;
                        break;
                    case '*':		//ラベル
                        if (IsKagLineHead(c) == false)
                        {
                            break;	//ラベルではない
                        }

                        Debug.WriteLine("fold level=" + m_sci.GetFoldLevel(m_line));
                        m_foldingLevel = FOLDING_LABEL;
                        m_sci.SetFoldLevel(m_line, (int)(SC_FOLDLEVELHEADERFLAG + m_foldingLevel));
                        skipToEndOfLine();
                        setStyleCh(STYLE_LABEL);
                        isSetStyle = true;
                        break;
                    case '@':		//タグ
                    case '[':
                        if (IsKagLineHead(c) == false && c == '@')
                        {
                            break;	//'@'の時だけ行頭必須なので処理を飛ばす
                        }

                        startStyleTag();
                        isSetStyle = true;
                        break;
                    default:
                        break;
                }

                if (isSetStyle == false)
                {
                    setStyleCh(STYLE_DEFAULT);
                }
            }
        }

        /// <summary>
        /// タグのスタイル設定を行う
        /// </summary>
        private void startStyleTag()
        {
            int style = STYLE_TAG;
            char c;
            int nextChar;
            bool end = false;
            while ((nextChar = readerRead()) != -1)
            {
                c = (char)nextChar;
                switch (c)
                {
                    case '\t':
                    case ' ':
                        style = STYLE_TAG_ATTR;
                        break;
                    case '=':
                        style = STYLE_TAG_ATTR_VALUE;
                        break;
                    case '\"':
                        skipToEndOfDq();
                        break;
                    case '[':
                        style = STYLE_TAG;
                        break;
                    case ']':
                        //タグ終了
                        setStyleCh(STYLE_TAG);
                        end = true;
                        break;
                    case '\n':
                    case '\r':
                        //改行してタグ終了
                        handleLineEnd(c);
                        end = true;
                        break;
                    default:
                        end = false;
                        break;
                }

                if (end)
                {
                    break;	//終了
                }

                setStyleCh(style);
            }

            setStyleCh(style);
        }

        /// <summary>
        /// 次のダブルクォーテーションまで読み飛ばす（エスケープ有）
        /// </summary>
        private void skipToEndOfDq()
        {
            char c;
            int nextChar;
            while ((nextChar = readerRead()) != -1)
            {
                c = (char)nextChar;
                if (c == '\\')  //エスケープ？
                {
                    if ((char)readerPeek() == '\"')
                    {
                        readerRead();   //読み飛ばす
                    }
                }
                else if (c == '\"')
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 指定したスタイルを設定する
        /// </summary>
        /// <param name="style"></param>
        private void setStyleCh(int style)
        {
            if ((m_index - m_startPos) <= 0)
            {
                return;
            }

            m_sci.SetStyling(m_index - m_startPos, style);
            m_startPos = m_index;
        }
        #endregion
    }
}
