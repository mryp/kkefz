using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using PluginCore.Controls;
using PluginCore;
using System.Drawing;
using System.Diagnostics;
using ScintillaNet;
using KagContext.complete;
using PluginCore.Helpers;

namespace KagContext
{
	/// <summary>
	/// KAG入力補完クラス
	/// </summary>
	public class KagComplete
	{
		/// <summary>
		/// KAGの入力補完を行うかどうか
		/// </summary>
		private bool m_showComplete = false;

		/// <summary>
		/// 入力補完プロバイダ
		/// </summary>
		private KagCompletionDataProvider m_compProvider;

		/// <summary>
		/// 現在のエディタコントロール
		/// </summary>
		public ScintillaControl CurrentSci
		{
			get;
			set;
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public KagComplete()
		{
			m_showComplete = false;
			m_compProvider = new KagCompletionDataProvider();
			UITools.Manager.OnCharAdded += new UITools.CharAddedHandler(OnChar);
			UITools.Manager.OnMouseHover += new UITools.MouseHoverHandler(OnMouseHover);
		}

		/// <summary>
		/// 切り替え初期化
		/// </summary>
		/// <param name="sci"></param>
		public void Init(ScintillaNet.ScintillaControl sci)
		{
			if (sci != null && sci.ConfigurationLanguage == "kag")
			{
				m_showComplete = true;
			}
			else
			{
				m_showComplete = false;
			}
			this.CurrentSci = sci;
		}

		/// <summary>
		/// ショートカットキーイベント
		/// </summary>
		/// <param name="keys"></param>
		/// <returns></returns>
		public bool OnShortCutKey(Keys keys)
		{
			if (!m_showComplete)
			{
				return false;
			}
			if (keys == (Keys.Control | Keys.Space))
			{
				showCompletion(m_compProvider.GenerateCompletionData(this.CurrentSci, '\0'));
				return true;
			}

			return false;
		}
		
		/// <summary>
		/// 文字入力イベント
		/// </summary>
		/// <param name="sci"></param>
		/// <param name="value"></param>
		public void OnChar(ScintillaControl sci, Int32 value)
		{
			if (!m_showComplete)
			{
				return;
			}

			switch ((char)value)
			{
				case '[':
					showCompletion(m_compProvider.GenerateCompletionData(this.CurrentSci, (char)value));
					SnippetHelper.InsertSnippetText(sci, sci.CurrentPos, "]");
					break;
				case '@':
				case ' ':
				case '=':
					showCompletion(m_compProvider.GenerateCompletionData(this.CurrentSci, (char)value));
					break;
			}
		}

		/// <summary>
		/// 入力補完リスト表示
		/// </summary>
		/// <param name="itemList"></param>
		private void showCompletion(List<ICompletionListItem> itemList)
		{
			if (itemList == null || itemList.Count == 0)
			{
				//表示項目がないので何もしない
				return;
			}

			if (string.IsNullOrEmpty(m_compProvider.PreSelection))
			{
				//選択ワードがないとき
				CompletionList.Show(itemList, true);
			}
			else if (this.CurrentSci.MBSafeTextLength(m_compProvider.PreSelection) != m_compProvider.PreSelection.Length)
			{
				//日本語単語選択時
				this.CurrentSci.SelectWord();
				CompletionList.Show(itemList, true);
			}
			else
			{
				//通常選択時
				CompletionList.Show(itemList, true, m_compProvider.PreSelection);
			}
		}

		private void OnMouseHover(ScintillaNet.ScintillaControl sci, int position)
		{
			if (!m_showComplete)
			{
				return;
			}

			//現在のカーソル位置にある
			string toolTip = KagToolTip.GetText(sci, position);
			if (string.IsNullOrEmpty(toolTip))
			{
				return;
			}

			UITools.Tip.ShowAtMouseLocation(toolTip);
		}
	}
}
