using System;
using System.Collections.Generic;
using System.Text;

using G_PROJECT;

namespace KagContext.io
{
	class TextEncoding : TxtEnc
	{
		#region フィールド
        private static TextEncoding m_instance = null;
        private Encoding defaultEncoding = Encoding.GetEncoding("shift_jis");
		#endregion

		#region プロパティ
		/// <summary>
		/// 結果が不明のときとりあえず返すデフォルトのエンコード
		/// </summary>
		public Encoding DefaultEncoding
		{
			get { return defaultEncoding; }
			set { defaultEncoding = value; }
		}

        public static TextEncoding Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new TextEncoding();
                }
                return m_instance;
            }
        }
		#endregion

		#region メソッド
		/// <summary>
		/// コンストラクタ
		/// </summary>
		private TextEncoding()
			: base()
		{
		}

		/// <summary>
		/// ファイル名からエンコードを判別して返す
		/// </summary>
		/// <param name="fileName">ファイル名</param>
		/// <returns>文字エンコーディング</returns>
		public Encoding GetFileEncoding(string fileName)
		{
			System.Text.Encoding enc = this.SetFromTextFile(fileName);
			if (enc == null)
			{
				return defaultEncoding;
			}
			else
			{
				return enc;
			}
		}
		#endregion
	}

	/// <summary>
	/// テキストエンコーディングの表示や状態を保持する構造体
	/// </summary>
	public struct TextEncodingState
	{
		#region フィールド
		/// <summary>
		/// エンコード名
		/// </summary>
		string name;
		#endregion

		#region プロパティ
		/// <summary>
		/// 保存している改行コード名
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
		}
		#endregion

		#region メソッド
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="name">エンコーディングの名前</param>
		public TextEncodingState(string name)
		{
			switch (name)
			{
				case "shift_jis":
                case "utf-16":
                case "utf-8":
					this.name = name;
					break;
				default:
					this.name = "shift_jis";
					break;
			}
		}

		/// <summary>
		/// 文字列化
		/// </summary>
		/// <returns>文字列</returns>
		public override string ToString()
		{
			string result = "";
			switch (name)
			{
				case "shift_jis":
					result = Encoding.GetEncoding(name).EncodingName;
					break;
				case "utf-16":
					result = Encoding.Unicode.EncodingName;
                    break;
                case "utf-8":
                    result = Encoding.UTF8.EncodingName;
                    break;
				default:
					result = "？不明？";
					break;
			}

			return result;
		}
		#endregion
	}
}
