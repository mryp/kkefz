using System;
using System.Collections.Generic;
using System.Text;
using PluginCore;
using System.Drawing;

namespace KagContext.complete
{
    public class KagCompletionListItem : ICompletionListItem
    {
        private string m_label;
        private string m_value;
        private string m_description;
        private Bitmap m_icon;

        public KagCompletionListItem(string label, string value, string description, Bitmap bmp)
        {
            m_label = label;
            m_value = value;
            m_description = description;
            m_icon = bmp;
        }

        public string Label
        {
            get { return m_label; }
        }

        public string Value
        {
            get { return m_value; }
        }

        public string Description
        {
            get { return m_description; }
        }

        public Bitmap Icon
        {
            get { return m_icon; }
        }
    }
}
