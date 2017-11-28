using System;
using System.Collections.Generic;
using System.Text;
using PluginCore;
using System.Drawing;
using System.ComponentModel;
using PluginCore.Utilities;
using System.IO;
using PluginCore.Managers;
using PluginCore.Helpers;
using System.Diagnostics;
using KrkrzPlugin.io;
using System.Windows.Forms;

namespace KrkrzPlugin
{
	public class PluginMain : IPlugin
	{
		#region フィールド
		private String m_pluginName = "KrkrzPlugin";
		private String m_pluginGuid = "18280428-07D6-4DFA-9565-A550E778AB78";
		private String m_pluginHelp = "www.poringsoft.net/";
		private String m_pluginDesc = "KrkrZ Plugin";
		private String m_pluginAuth = "PORING SOFT";
		private String m_settingFilename;
		private Image m_pluginImage;
		private static Settings m_settingObject;
		private Context2 contextInstance;
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
			String dataPath = Path.Combine(PathHelper.DataDir, "TjsContext");
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
			EventManager.AddEventHandler(this, EventType.UIStarted | EventType.Command | EventType.ProcessStart | EventType.ProcessEnd | EventType.ProcessArgs);
			//EventManager.AddEventHandler(this, EventType.Command, HandlingPriority.High);
			//EventManager.AddEventHandler(this, EventType.Command | EventType.Keys | EventType.ProcessArgs, HandlingPriority.Low);
			//EventManager.AddEventHandler(this, (EventType)0x0FFFFFFFFFFFFFFF);
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
		/// 保存用デフォルト文字コード
		/// </summary>
		private Encoding DEF_ENC = new UnicodeEncoding(false, true);

		/// <summary>
		/// 実行プロセス
		/// </summary>
		private Process m_process = null;

		/// <summary>
		/// エラー発生時にキャッチする実行ファイルパス（FlashDevelop.exeからの相対パス）
		/// </summary>
		private const string ERROR_CATCH_EXE = "Tools\\krkrz\\krkrerrorcatch.exe";

		/// <summary>
		/// エラー発生時にキャッチする実行ファイルパスのキャッシュ用変数
		/// </summary>
		private string m_errorCatchExePath = "";

		/// <summary>
		/// 最後に発生したエラーメッセージ行
		/// </summary>
		private string m_lastErrorMessage = "";

		/// <summary>
		/// デバッグモードが選択されているかどうか
		/// </summary>
		private bool m_debugFlag = false;

		/// <summary>
		/// プラグインイベント処理
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <param name="priority"></param>
		public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
		{
			switch (e.Type)
			{
				case EventType.UIStarted:
					contextInstance = new Context2(EnvSettings);
					ASCompletion.Context.ASContext.RegisterLanguage(contextInstance, "tjs");
					break;
				case EventType.Command:
					DataEvent de = e as DataEvent;
					if (PluginBase.CurrentProject == null)
					{
						break;
					}

					string action = de.Action;
					Debug.WriteLine("TjsContext.PluginMain#HandleEvent CommandAction=" + action);
					if (action == "ProjectManager.Project")
					{
						contextInstance.BuildClassPath();
					}
					else if (action == "ProjectManager.TestingProject")
					{
						Debug.WriteLine(de.Data);
						if (de.Data.ToString() == "Debug")
						{
							m_debugFlag = true;
						}
						else
						{
							m_debugFlag = false;
						}
					}
					else if (action == "ProjectManager.RunCustomCommand")
					{
						de.Handled = true;
						startProcess(de.Data.ToString(), m_debugFlag);
					}
					break;
				case EventType.ProcessStart:
					break;
				case EventType.ProcessEnd:
					break;
				case EventType.ProcessArgs:
					break;
			}
		}

		/// <summary>
		/// プログラムを実行する
		/// </summary>
		/// <param name="fileName">プログラムファイル名</param>
		private void startProcess(string fileName, bool debugFlag)
		{
			TraceManager.AddAsync("startProcess fileName=" + fileName);
			if (m_process != null)
			{
				if (m_process.HasExited == false)
				{
					m_process.Kill();
				}
				m_process = null;
			}
			m_process = new Process();
			m_process.StartInfo.UseShellExecute = false;
			m_process.StartInfo.RedirectStandardOutput = true;
			m_process.StartInfo.RedirectStandardInput = false;
			m_process.OutputDataReceived += process_OutputDataReceived;

			m_process.StartInfo.FileName = fileName;
			string exceptionArg = String.Format("-exceptionexe={0} -exceptionarg={1}"
				, getErrorCatchExePath(), "%filepath%(%line%)");
			if (debugFlag)
			{
				exceptionArg += String.Format(" -debug=yes");
			}
			m_process.StartInfo.Arguments = exceptionArg;
			m_process.StartInfo.WorkingDirectory = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
			m_process.Start();

			m_process.BeginOutputReadLine();
		}

		/// <summary>
		/// エラーキャッチ用プログラムパス
		/// </summary>
		/// <returns></returns>
		private string getErrorCatchExePath()
		{
			if (string.IsNullOrEmpty(m_errorCatchExePath))
			{
				m_errorCatchExePath = Path.Combine(PathHelper.BaseDir, ERROR_CATCH_EXE).ToLower();
			}
			return m_errorCatchExePath;
		}

		/// <summary>
		/// 標準出力のキャッチ
		/// </summary>
		private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			string line = e.Data;
			if (string.IsNullOrEmpty(line))
			{
				line = "";
			}

			TraceType traceType = TraceType.Debug;
			int checkPos = -1;
			string checkLine = line.ToLower();
			if ((checkPos = checkLine.IndexOf(getErrorCatchExePath())) != -1)
			{
				//エラー結果を表示する
				traceType = TraceType.Error;
				if (line.Length > checkPos + getErrorCatchExePath().Length + 1)
				{
					line = line.Substring(checkPos + getErrorCatchExePath().Length + 1).Replace("\"", "") + " : Error: " + m_lastErrorMessage;
				}
			}
			else if (checkLine.IndexOf("error") != -1
			|| checkLine.IndexOf("エラー") != -1)
			{
				traceType = TraceType.Error;
				m_lastErrorMessage = line;
			}

			//出力ウィンドウに表示
			TraceManager.AddAsync(line, (int)traceType);
		}
		#endregion
	}
}
