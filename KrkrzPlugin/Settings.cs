using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using PluginCore;
using PluginCore.Localization;

namespace KrkrzPlugin
{
	[Serializable]
	public class Settings : ASCompletion.Settings.IContextSettings
    {
		#region IContextSettings メンバー

        [Browsable(false)]
		public string LanguageId
		{
			get { return "TJS"; }
		}

        [Browsable(false)]
		public string DefaultExtension
		{
			get { return ".tjs"; }
		}
        
        [Browsable(false)]
        public string CheckSyntaxRunning
        {
            get { return ""; }
        }

        [Browsable(false)]
        public string CheckSyntaxDone
        {
            get { return ""; }
        }

		const string DEFAULT_DOC_COMMAND =
			"http://google.com/search?q=$(ItmTypName) $(ItmName) site:devdoc.kikyou.info";
		protected string documentationCommandLine = DEFAULT_DOC_COMMAND;
        
        [DisplayName("Documentation Command Line")]
        [LocalizedCategory("ASCompletion.Category.Documentation"), LocalizedDescription("ASCompletion.Description.DocumentationCommandLine"), DefaultValue(DEFAULT_DOC_COMMAND)]
        public string DocumentationCommandLine
		{
			get { return documentationCommandLine; }
			set { documentationCommandLine = value; }
		}

		const bool DEFAULT_COMPLETIONENABLED = true;
		protected bool completionEnabled = DEFAULT_COMPLETIONENABLED;
        [DisplayName("Enable Completion")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.CompletionEnabled"), DefaultValue(DEFAULT_COMPLETIONENABLED)]
        public bool CompletionEnabled
		{
			get { return completionEnabled; }
			set { completionEnabled = value; }
		}

		protected string[] userClasspath = null;
		[DisplayName("User Classpath")]
		[LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.UserClasspath")]
		public string[] UserClasspath
		{
			get { return userClasspath; }
			set
			{
				userClasspath = value;
			}
		}

		const bool DEFAULT_GENERATEIMPORTS = false;
		protected bool generateImports = DEFAULT_GENERATEIMPORTS;
        [DisplayName("Generate Imports")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.GenerateImports"), DefaultValue(DEFAULT_GENERATEIMPORTS)]
        public bool GenerateImports
		{
			get { return generateImports; }
			set { generateImports = value; }
		}

		const bool DEFAULT_LAZYMODE = false;
        private bool lazyClasspathExploration = DEFAULT_LAZYMODE;
        [DisplayName("Lazy Classpath Exploration")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.LazyClasspathExploration"), DefaultValue(DEFAULT_LAZYMODE)]
        public bool LazyClasspathExploration
		{
			get { return lazyClasspathExploration; }
			set { lazyClasspathExploration = value; }
		}

		const bool DEFAULT_LISTALL = true;
		protected bool completionListAllTypes = DEFAULT_LISTALL;
        [DisplayName("List All Types In Completion")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.CompletionListAllTypes"), DefaultValue(DEFAULT_LISTALL)]
        public bool CompletionListAllTypes
		{
			get { return completionListAllTypes; }
			set { completionListAllTypes = value; }
		}

		const bool DEFAULT_QUALIFY = false;
		protected bool completionShowQualifiedTypes = DEFAULT_QUALIFY;
        [DisplayName("Show Qualified Types In Completion")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.CompletionShowQualifiedTypes"), DefaultValue(DEFAULT_QUALIFY)]
        public bool CompletionShowQualifiedTypes
		{
			get { return completionShowQualifiedTypes; }
			set { completionShowQualifiedTypes = value; }
		}

		const bool DEFAULT_CHECKSYNTAX = false;
		protected bool checkSyntaxOnSave = DEFAULT_CHECKSYNTAX;
        [DisplayName("Check Syntax On Save")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.CheckSyntaxOnSave"), DefaultValue(DEFAULT_CHECKSYNTAX)]
        public bool CheckSyntaxOnSave
		{
			get { return checkSyntaxOnSave; }
			set { checkSyntaxOnSave = value; }
		}

		const bool DEFAULT_PLAY = false;
		protected bool playAfterBuild = DEFAULT_PLAY;
        [DisplayName("Play After Build")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.PlayAfterBuild"), DefaultValue(DEFAULT_PLAY)]
        public bool PlayAfterBuild
		{
			get { return playAfterBuild; }
			set { playAfterBuild = value; }
		}

		const bool DEFAULT_FIXPACKAGEAUTOMATICALLY = false;
        protected bool fixPackageAutomatically = DEFAULT_FIXPACKAGEAUTOMATICALLY;
        [DisplayName("Fix Package Automatically")]
        [LocalizedCategory("ASCompletion.Category.Common"), LocalizedDescription("ASCompletion.Description.FixPackageAutomatically"), DefaultValue(DEFAULT_FIXPACKAGEAUTOMATICALLY)]
        public bool FixPackageAutomatically
		{
			get { return fixPackageAutomatically; }
			set { fixPackageAutomatically = value; }
		}

		protected PluginCore.InstalledSDK[] installedSDKs = null;
        /*
        [DisplayName("Installed MTASC SDKs")]
        [LocalizedCategory("ASCompletion.Category.Language"), LocalizedDescription("AS2Context.Description.MtascPath")]
        */
        [Browsable(false)]
        public PluginCore.InstalledSDK[] InstalledSDKs
		{
			get { return installedSDKs; }
			set
			{
				installedSDKs = value;
			}
		}

		public PluginCore.InstalledSDK GetDefaultSDK()
		{
			if (installedSDKs == null || installedSDKs.Length == 0)
				return InstalledSDK.INVALID_SDK;

			foreach (InstalledSDK sdk in installedSDKs)
				if (sdk.IsValid) return sdk;
			return InstalledSDK.INVALID_SDK;
		}

		#endregion
	}
}
