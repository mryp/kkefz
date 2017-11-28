using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace KrkrzPlugin.io
{
	/// <summary>
	/// ファイル関連のユーティリティクラス
	/// </summary>
	public class FileUtil
	{
		/// <summary>
		/// ディレクトリの中身をコピーする
		/// </summary>
		/// <param name="sourceDirName">コピー元のディレクトリ</param>
		/// <param name="destDirName">コピー先のディレクトリ（指定されたディレクトリがないときは作成する）</param>
		public static void CopyDirectory(string sourceDirName, string destDirName)
		{
			//コピー先のディレクトリがないときは作る
			if (Directory.Exists(destDirName) == false)
			{
				Directory.CreateDirectory(destDirName);
				File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));
			}

			//コピー先のディレクトリ名の末尾に"\"をつける
			if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
			{
				destDirName = destDirName + Path.DirectorySeparatorChar;
			}

			//コピー元のディレクトリにあるファイルをコピー
			string[] files = Directory.GetFiles(sourceDirName);
			foreach (string file in files)
			{
				File.Copy(file, destDirName + Path.GetFileName(file), true);
			}

			//コピー元のディレクトリにあるディレクトリについて、
			//再帰的に呼び出す
			string[] dirs = Directory.GetDirectories(sourceDirName);
			foreach (string dir in dirs)
			{
				CopyDirectory(dir, destDirName + Path.GetFileName(dir));
			}
		}

		/// <summary>
		/// 指定されたファイル名がファイル名として有効かをチェックする
		/// </summary>
		/// <param name="fileName">チェックしたいファイル名</param>
		/// <returns>有効なときtrue、無効な文字が含まれていたときfalse</returns>
		public static bool IsValidFileName(string fileName)
		{
			char[] invalidChars = Path.GetInvalidFileNameChars();
			foreach (char c in invalidChars)
			{
				//無効な文字があるかどうか
				if (fileName.Contains(c.ToString()) == true)
				{
					//無効な文字を発見！
					return false;
				}
			}

			//無効な文字が見つからなかったので、このファイル名は有効とする
			return true;
		}

		/// <summary>
		/// ベースパスからの相対パスを絶対パスに変換する
		/// </summary>
		/// <param name="basePath">ベースパス</param>
		/// <param name="path">相対パス</param>
		/// <returns>絶対パス</returns>
		public static string ConvertFullPath(string basePath, string relativePath)
		{
			if (basePath == "")
			{
				return relativePath;
			}

			Uri baseUri = new Uri(basePath + "\\");
			Uri fullUri = new Uri(baseUri, relativePath);

			return fullUri.LocalPath;
		}

		/// <summary>
		/// 絶対パスをベースパスからの相対パスに変換する
		/// </summary>
		/// <param name="path">絶対パス</param>
		/// <returns>相対パス</returns>
		public static string ConvertRelativaPath(string basePath, string fullPath)
		{
			if (basePath == "")
			{
				return fullPath;
			}

			Uri u1 = new Uri(basePath + "\\");
			Uri u2 = new Uri(u1, fullPath);
			string relativePath = u1.MakeRelativeUri(u2).ToString();
			relativePath = System.Web.HttpUtility.UrlDecode(relativePath).Replace('/', '\\');	//URLデコードして、'/'を'\'に変更する

			return relativePath;
		}

		/// <summary>
		/// ディレクトリ内のファイルから指定した拡張子のファイルをすべて取得する
		/// </summary>
		/// <param name="dirPath">検索開始するディレクトリパス</param>
		/// <param name="searchExt">検索する拡張子（例：*.tjs;*ks）</param>
		/// <param name="option">検索範囲</param>
		/// <returns>見つかったファイルパスリスト</returns>
		public static string[] GetDirectoryFile(string dirPath, string searchExt, SearchOption option)
		{
			if (Directory.Exists(dirPath) == false)
			{
				return null;	//検索するディレクトリが無い
			}
			if (searchExt == null || searchExt.Length == 0)
			{
				return null;	//検索する拡張子がない
			}

			//検索する拡張子を取得する
			string[] extList = searchExt.Split(';');
			if (extList == null || extList.Length == 0)
			{
				return null;
			}

			//拡張子ごとにファイルを検索しリストにセットする
			List<string> pathList = new List<string>();
			foreach (string ext in extList)
			{
				string[] list = Directory.GetFiles(dirPath, ext, option);
				if (list != null)
				{
					pathList.AddRange(list);
				}
			}

			pathList.Sort();
			return pathList.ToArray();
		}
	}
}
