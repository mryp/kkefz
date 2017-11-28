using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PluginCore.Helpers;
using PluginCore;

namespace KagContext
{
	public class EnvPath
	{
		#region 定数
		private const string FILE_NAME_DEF_KAG = "kag3.ks";

		public enum TargetPlatform
		{
			Kag3,
			Kagex,
		}
		#endregion

		#region フィールド
		private static EnvPath m_instance = null;
		private string m_kagDefFilePath = "";
		#endregion

		/// <summary>
		/// パス関連取得クラスの唯一のオブジェクト
		/// </summary>
		public static EnvPath Instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = new EnvPath();
				}

				return m_instance;
			}
		}

		/// <summary>
		/// コンストラクタ（非表示）
		/// </summary>
		private EnvPath()
		{
		}

		/// <summary>
		/// KAG用定義ファイル（入力補完用）パス
		/// </summary>
		public string KagDefFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(m_kagDefFilePath))
				{
					m_kagDefFilePath = Path.Combine(PathHelper.LibraryDir, "Kag\\tag\\" + FILE_NAME_DEF_KAG);
					if (File.Exists(m_kagDefFilePath) == false)
					{
						m_kagDefFilePath = Path.Combine(PathHelper.UserLibraryDir, "Kag\\tag\\" + FILE_NAME_DEF_KAG);
					}
				}

				return m_kagDefFilePath;
			}
		}

		/// <summary>
		/// 現在のカレントプロジェクトにあるKagデータフォルダパス
		/// プロジェクトが読み込まれていない等の時は空文字を返す
		/// </summary>
		/// <returns></returns>
		public string ProjectKagDataDirPath
		{
			get
			{
				if (PluginBase.CurrentProject == null || string.IsNullOrEmpty(PluginBase.CurrentProject.OutputPathAbsolute))
				{
					return "";
				}

				return Path.Combine(PluginBase.CurrentProject.OutputPathAbsolute, "data");
			}
		}

		/// <summary>
		/// 現在のカレントプロジェクトのあるフォルダパス
		/// </summary>
		public string ProjectDirPath
		{
			get
			{
				if (PluginBase.CurrentProject == null || string.IsNullOrEmpty(PluginBase.CurrentProject.OutputPathAbsolute))
				{
					return "";
				}

				return PluginBase.CurrentProject.OutputPathAbsolute;
			}
		}

		/// <summary>
		/// プロジェクトのプラットフォーム情報を取得する
		/// </summary>
		public TargetPlatform ProjectPlatform
		{
			get
			{
				return TargetPlatform.Kag3;
			}
		}
	}
}
