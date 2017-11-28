using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using KagContext.parse;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using PluginCore.Controls;
using KagContext.io;
using System.Threading;

namespace KagContext
{
	public class PluginMain : IPlugin
	{
		#region フィールド
		private String m_pluginName = "KagPlugin";
		private String m_pluginGuid = "1DF668DF-3E79-4AB0-88D4-5B9CAB35552F";
		private String m_pluginHelp = "www.poringsoft.net/";
		private String m_pluginDesc = "KAG3 Plugin";
		private String m_pluginAuth = "PORING SOFT";
		private String m_settingFilename;
		private Image m_pluginImage;
		private static Settings m_settingObject;
		#endregion

		#region プロパティ
		/// <summary>
		/// APIレベル（FP4のときは1）
		/// </summary>
		public int Api
		{
			get { return 1; }
		}

		/// <summary>
		/// プラグイン名
		/// </summary>
		public string Name
		{
			get { return m_pluginName; }
		}

		/// <summary>
		/// プラグインGUID
		/// </summary>
		public string Guid
		{
			get { return m_pluginGuid; }
		}

		/// <summary>
		/// ヘルプURL
		/// </summary>
		public string Help
		{
			get { return m_pluginHelp; }
		}

		/// <summary>
		/// プラグイン作者
		/// </summary>
		public string Author
		{
			get { return m_pluginAuth; }
		}

		/// <summary>
		/// プラグイン説明
		/// </summary>
		public string Description
		{
			get { return m_pluginDesc; }
		}

		/// <summary>
		/// 設定オブジェクト
		/// </summary>
		[Browsable(false)]
		public object Settings
		{
			get { return m_settingObject; }
		}

		/// <summary>
		/// 設定オブジェクト
		/// </summary>
		public static Settings EnvSettings
		{
			get
			{
				return m_settingObject;
			}
		}
		#endregion

		#region メソッド
		/// <summary>
		/// 初期化
		/// </summary>
		public void Initialize()
		{
			InitBasics();
			AddEventHandlers();
			LoadSettings();

			m_kagStyle = new KagStyleLexer();
			m_kagComplete = new KagComplete();
		}

		/// <summary>
		/// 終了処理
		/// </summary>
		public void Dispose()
		{
			this.SaveSettings();
		}

		/// <summary>
		/// プラグイン初期設定
		/// </summary>
		public void InitBasics()
		{
			String dataPath = Path.Combine(PathHelper.DataDir, "KagPlugin");
			if (!Directory.Exists(dataPath))
			{
				Directory.CreateDirectory(dataPath);
			}

			this.m_settingFilename = Path.Combine(dataPath, "Settings.fdb");
			this.m_pluginImage = PluginBase.MainForm.FindImage("100");
		}

		/// <summary>
		/// イベント追加
		/// </summary>
		public void AddEventHandlers()
		{
			EventType eventType = EventType.FileOpen 
				| EventType.FileSwitch 
				| EventType.SyntaxChange 
				| EventType.SettingChanged
				| EventType.ApplySettings
				| EventType.Keys
				| EventType.Completion
				| EventType.Command;
			EventManager.AddEventHandler(this, eventType);
		}

		/// <summary>
		/// 設定の読み込み
		/// </summary>
		public void LoadSettings()
		{
			m_settingObject = new Settings();
			if (!File.Exists(this.m_settingFilename))
			{
				this.SaveSettings();
			}
			else
			{
				Object obj = ObjectSerializer.Deserialize(this.m_settingFilename, m_settingObject);
				m_settingObject = (Settings)obj;
			}
		}

		/// <summary>
		/// 設定の保存
		/// </summary>
		public void SaveSettings()
		{
			ObjectSerializer.Serialize(this.m_settingFilename, m_settingObject);
		}
		#endregion

		#region イベント処理
		/// <summary>
		/// KAGスタイル管理
		/// </summary>
		private KagStyleLexer m_kagStyle = null;

		/// <summary>
		/// KAG入力補完管理
		/// </summary>
		private KagComplete m_kagComplete = null;

		/// <summary>
		/// パーサーオブジェクト（グローバル）
		/// </summary>
		private static ParserService m_parser = new ParserService();

		/// <summary>
		/// 保存用デフォルト文字コード
		/// </summary>
		private Encoding DEF_ENC = new UnicodeEncoding(false, true);

		/// <summary>
		/// リフレッシュ待ちタイマー
		/// </summary>
		private System.Threading.Timer m_delayRefreshTimer = null;

		/// <summary>
		/// リフレッシュ待ち時間（ミリ秒）
		/// </summary>
		private int m_delayTime = 1000;

		/// <summary>
		/// KAG解析情報・保持オブジェクト
		/// </summary>
		public static ParserService ParserSrv
		{
			get
			{
				return m_parser;
			}
		}

		/// <summary>
		/// プラグインイベント処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <param name="priority"></param>
		public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
		{
			Debug.WriteLine("KagContext.PluginMain#HandleEvent type=" + e.Type.ToString());
			switch (e.Type)
			{
				case EventType.FileSwitch:
				case EventType.SyntaxChange:
				case EventType.SettingChanged:
				case EventType.ApplySettings:
					if (PluginBase.MainForm.CurrentDocument != null
					&&  PluginBase.MainForm.CurrentDocument.SciControl != null)
					{
						m_kagStyle.Init(PluginBase.MainForm.CurrentDocument.SciControl);
						m_kagComplete.Init(PluginBase.MainForm.CurrentDocument.SciControl);
						m_kagStyle.CurrentFile = PluginBase.MainForm.CurrentDocument.FileName;
						delayStyleRefresh();
					}
					break;
				case EventType.Keys:
					e.Handled = m_kagComplete.OnShortCutKey(((KeyEvent)e).Value);
					break;
				case EventType.Completion:
					//サジェストを発生させずKAG用の入力補完を使用する
					e.Handled = true;
					break;
				case EventType.Command:
					DataEvent de = e as DataEvent;
					string action = de.Action;
					Debug.WriteLine("KagContext.PluginMain#HandleEvent CommandAction=" + action);
					if (action == "ProjectManager.Project") //プロジェクト読み込み時
					{
						string[] defFilePathList = new string[] { };
						if (EnvPath.Instance.ProjectPlatform == EnvPath.TargetPlatform.Kag3)
						{
							defFilePathList = new string[] { EnvPath.Instance.KagDefFilePath };
						}

						ParserSrv.ParseDirectory(EnvPath.Instance.ProjectKagDataDirPath, defFilePathList);
					}

					//string projectPath = ((System.Collections.Hashtable)de.Data)["project"].ToString();
					break;
			}
		}

		private void delayStyleRefresh()
		{
			if (m_delayRefreshTimer != null)
			{
				m_delayRefreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
				m_delayRefreshTimer.Dispose();
				m_delayRefreshTimer = null;
			}

			m_delayRefreshTimer = new System.Threading.Timer(new TimerCallback((object obj) =>
			{
				if (m_delayRefreshTimer == null)
				{
					//すでにタイマーが止められているので何もしない
					return;
				}

				if (PluginBase.MainForm.CurrentDocument != null)
				{
					m_kagStyle.RefreshStyle(PluginBase.MainForm.CurrentDocument.SciControl);
				}
			}), null, m_delayTime, Timeout.Infinite);
		}
		#endregion
	}
}
