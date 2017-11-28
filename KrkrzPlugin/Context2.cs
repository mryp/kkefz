using System;
using System.Collections.Generic;
using System.Text;
using ASCompletion.Model;
using System.IO;
using PluginCore.Helpers;
using ASCompletion.Context;
using ASCompletion.Completion;
using System.Text.RegularExpressions;
using PluginCore;
using System.Diagnostics;

namespace KrkrzPlugin
{
	public class Context2 : AS2Context.Context
	{
		/// <summary>
		/// TJS関連を含む設定情報
		/// </summary>
		private Settings tjsSetting;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="initSettings"></param>
		public Context2(Settings initSettings)
		{
			/* DESCRIBE LANGUAGE FEATURES */
			tjsSetting = initSettings;
			docType = "System";

			// language constructs
			features.hasPackages = false;
			features.hasNamespaces = false;
			features.hasImports = false;
			features.hasImportsWildcard = false;
			features.hasClasses = true;
			features.hasExtends = true;
			features.hasImplements = false;
			features.hasInterfaces = false;
			features.hasEnums = false;
			features.hasGenerics = false;
			features.hasEcmaTyping = true;
			features.hasVars = true;
			features.hasConsts = true;
			features.hasMethods = true;
			features.hasStatics = true;
			features.hasOverride = true;
			features.hasTryCatch = false;
			features.hasE4X = false;
			features.hasStaticInheritance = true;
			features.checkFileName = false;

			// allowed declarations access modifiers
			Visibility all = Visibility.Public | Visibility.Internal | Visibility.Protected | Visibility.Private;
			features.classModifiers = all;
			features.varModifiers = all;
			features.constModifiers = all;
			features.methodModifiers = all;

			// default declarations access modifiers
			features.classModifierDefault = Visibility.Public;
			features.varModifierDefault = Visibility.Public;
			features.methodModifierDefault = Visibility.Public;

			// keywords
			features.dot = ".";
			features.voidKey = "void";
			features.objectKey = "Object";
			features.booleanKey = "Boolean";
			features.numberKey = "Number";
			features.arrayKey = "Array";
			features.importKey = "import";
			features.typesPreKeys = new string[] { "incontextof", "new", "instanceof", "extends" };
			features.codeKeywords = new string[] { 
				"break",
				"continue",
				"const",
				"catch",
				"class",
				"case",
				"debugger",
				"default",
				"delete",
				"do",
				"extends",
				"export",
				"enum",
				"else",
				"function",
				"finally",
				"false",
				"for",
				"global",
				"getter",
				"goto",
				"incontextof",
				"Infinity",
				"invalidate",
				"instanceof",
				"isvalid",
				"import",
				"int",
				"in",
				"if",
				"NaN",
				"null",
				"new",
				"octet",
				"protected",
				"property",
				"private",
				"public",
				"return",
				"real",
				"synchronized",
				"switch",
				"static",
				"setter",
				"string",
				"super",
				"typeof",
				"throw",
				"this",
				"true",
				"try",
				"void",
				"var",
				"while",
				"with",
			};
			features.varKey = "var";
			features.constKey = "const";
			features.functionKey = "function";
			features.getKey = "get";
			features.setKey = "set";
			features.staticKey = "static";
			features.finalKey = "final";
			features.overrideKey = "override";
			features.publicKey = "public";
			features.internalKey = "internal";
			features.protectedKey = "protected";
			features.privateKey = "private";
			features.intrinsicKey = "extern";
			features.namespaceKey = "namespace";

			/* INITIALIZATION */

			settings = initSettings;
		}

		/// <summary>
		/// クラスパスを生成する
		/// </summary>
		public override void BuildClassPath()
		{
			ReleaseClasspath();
			started = true;
			if (contextSetup == null)
			{
				contextSetup = new ContextSetupInfos();
				contextSetup.Lang = settings.LanguageId;
				contextSetup.Platform = "KiriKiriZ";
				contextSetup.Version = "1.0.0.0";
			}
						
			//
			// Class pathes
			//
			classPath = new List<PathModel>();
			
			// add external pathes
			List<PathModel> initCP = classPath;
			classPath = new List<PathModel>();
			if (contextSetup.Classpath != null)
			{
				foreach (string cpath in contextSetup.Classpath)
				{
					AddPath(cpath.Trim());
				}
			}

			//ライブラリパス追加
			string tjsClassPath = Path.Combine(PathHelper.LibraryDir, "Tjs/classes");
			if (Directory.Exists(tjsClassPath) == false)
			{
				//ライブラリパスがないときはユーザーライブラリを探す
				tjsClassPath = Path.Combine(PathHelper.UserLibraryDir, "Tjs/classes");
			}
			AddPath(tjsClassPath);

			//プロジェクトディレクトリのパス追加
			if (PluginBase.CurrentProject != null 
			&& string.IsNullOrEmpty(PluginBase.CurrentProject.OutputPathAbsolute) == false)
			{
				string currentPath = Path.Combine(PluginBase.CurrentProject.OutputPathAbsolute, "data");
				if (Directory.Exists(currentPath))
				{
					AddPath(currentPath);
					string[] dirList = Directory.GetDirectories(currentPath, "*", SearchOption.TopDirectoryOnly);
					foreach (string subDir in dirList)
					{
						if (Directory.Exists(subDir))
						{
							AddPath(subDir);
						}
					}
				}
			}

			// add user pathes from settings
			if (settings.UserClasspath != null && settings.UserClasspath.Length > 0)
			{
				foreach(string cpath in settings.UserClasspath) AddPath(cpath.Trim());
			}
			// add initial pathes
			foreach(PathModel mpath in initCP) AddPath(mpath);

			// parse top-level elements
			InitTopLevelElements();
			if (cFile != null) UpdateTopLevelElements();
			
			// add current temporaty path
			if (temporaryPath != null)
			{
				string tempPath = temporaryPath;
				temporaryPath = null;
				SetTemporaryPath(tempPath);
			}
			FinalizeClasspath();
		}


		/// <summary>
		/// Prepare AS2 intrinsic known vars/methods/classes
		/// </summary>
		protected override void InitTopLevelElements()
		{
			string filename = "toplevel.tjs";
			topLevel = new FileModel(filename);

			// search top-level declaration
			foreach (PathModel aPath in classPath)
			{
				if (File.Exists(Path.Combine(aPath.Path, filename)))
				{
					filename = Path.Combine(aPath.Path, filename);
					topLevel = GetCachedFileModel(filename);
					break;
				}
			}

			if (File.Exists(filename))
			{
				// MTASC toplevel-style declaration:
				ClassModel tlClass = topLevel.GetPublicClass();
				if (!tlClass.IsVoid())
				{
					topLevel.Members = tlClass.Members;
					tlClass.Members = null;
					topLevel.Classes = new List<ClassModel>();
				}
			}
			// not found
			else
			{
			}

			if (topLevel.Members.Search("global", 0, 0) == null)
			{
				topLevel.Members.Add(new MemberModel("global", docType, FlagType.Variable, Visibility.Public));
			}
			if (topLevel.Members.Search("this", 0, 0) == null)
			{
				topLevel.Members.Add(new MemberModel("this", "", FlagType.Variable, Visibility.Public));
			}
			if (topLevel.Members.Search("super", 0, 0) == null)
			{
				topLevel.Members.Add(new MemberModel("super", "", FlagType.Variable, Visibility.Public));
			}
			if (topLevel.Members.Search(features.voidKey, 0, 0) == null)
			{
				topLevel.Members.Add(new MemberModel(features.voidKey, "", FlagType.Class | FlagType.Intrinsic, Visibility.Public));
			}

			topLevel.Members.Sort();
			foreach (MemberModel member in topLevel.Members)
			{
				member.Flags |= FlagType.Intrinsic;
			}
		}

		public override void ResolveTopLevelElement(string token, ASResult result)
		{
			if (topLevel != null && topLevel.Members.Count > 0)
			{
				// current class
				ClassModel inClass = ASContext.Context.CurrentClass;
				if (token == "this")
				{
					result.Member = topLevel.Members.Search("this", 0, 0);
					if (inClass.IsVoid())
						inClass = ASContext.Context.ResolveType(result.Member.Type, null);
					result.Type = inClass;
					result.InFile = ASContext.Context.CurrentModel;
					return;
				}
				else if (token == "super")
				{
					if (inClass.IsVoid())
					{
						MemberModel thisMember = topLevel.Members.Search("this", 0, 0);
						inClass = ASContext.Context.ResolveType(thisMember.Type, null);
					}
					inClass.ResolveExtends();
					ClassModel extends = inClass.Extends;
					if (!extends.IsVoid())
					{
						result.Member = topLevel.Members.Search("super", 0, 0);
						result.Type = extends;
						result.InFile = extends.InFile;
						return;
					}
				}
				else if (token == "global")
				{
					return;
				}

				// other top-level elements
				ASComplete.FindMember(token, topLevel, result, 0, 0);
				if (!result.IsNull()) return;
			}
		}

		public override void RemoveClassCompilerCache()
		{
		}

		/// <summary>
		/// 型情報を取得する
		/// </summary>
		/// <param name="cname"></param>
		/// <param name="inFile"></param>
		/// <returns></returns>
		public override ClassModel ResolveType(string cname, FileModel inFile)
		{
			//強制的にStatic属性をつける
			ClassModel classModel = base.ResolveType(cname, inFile);
			return setAllStaticToClassModel(classModel);
		}

		/// <summary>
		/// 指定した型情報にStatic属性をつけて返す
		/// </summary>
		/// <param name="classModel"></param>
		/// <returns></returns>
		private ClassModel setAllStaticToClassModel(ClassModel classModel)
		{
			if (classModel != null && classModel.Members.Items.Count > 0)
			{
				foreach (MemberModel item in classModel.Members.Items)
				{
					item.Flags = item.Flags | FlagType.Static;
				}
			}

			return classModel;
		}

		/// <summary>
		/// 指定した型情報からConstructor属性を削除する
		/// </summary>
		/// <param name="classModel"></param>
		/// <returns></returns>
		private ClassModel delAllConstructorToClassModel(ClassModel classModel)
		{
			if (classModel != null && classModel.Members.Items.Count > 0)
			{
				foreach (MemberModel item in classModel.Members.Items)
				{
					if ((item.Flags & FlagType.Constructor) == FlagType.Constructor)
					{
						item.Flags = item.Flags ^ FlagType.Constructor;
						item.Flags = item.Flags | FlagType.Dynamic;
					}
				}
			}

			return classModel;
		}
		
		/// <summary>
		/// パッケージ情報を返す
		/// </summary>
		/// <param name="name"></param>
		/// <param name="lazyMode"></param>
		/// <returns></returns>
		public override FileModel ResolvePackage(string name, bool lazyMode)
		{
			FileModel element = base.ResolvePackage(name, lazyMode);
			if (element == null || element.Imports == null || element.Imports.Items == null)
			{
				return element;
			}
			if (name == "System")
			{
				return null;
			}

			//パッケージを削除する
			element.Imports.Items.RemoveAll(item => item.Flags == FlagType.Package);
			return element;
		}

		/// <summary>
		/// クラス一覧を返す
		/// </summary>
		/// <returns></returns>
		public override MemberList GetAllProjectClasses()
		{
			MemberList memberList = this.GetVisibleExternalElements();
			return memberList;
		}

		public override FileModel CreateFileModel(string fileName)
		{
			FileModel model = base.CreateFileModel(fileName);
			model.HasFiltering = true;
			model.Context = this;
			return model;
		}

		public override string FilterSource(string fileName, string src)
		{
			//プロパティ記述を上書きする
			src = src.Replace("property", "function get");
			return src;
		}

		public override void FilterSource(FileModel model)
		{

		}

		public override void ExploreVirtualPath(PathModel path)
		{
			base.ExploreVirtualPath(path);
		}
	}
}
