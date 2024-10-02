

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace SharpBrowser {

	public enum FileIconSize : int {
		Large = 0x000000000,
		Small = 0x000000001
	}

	public static class FileIconUtils {


		public static MemoryStream GetFileIcon(string name, FileIconSize size) {
			Icon icon = FileIconUtils.IconFromExtension(name.GetAfter("."), size);
			using (icon) {
				using (var bmp = icon.ToBitmap()) {
					MemoryStream ms = new MemoryStream();
					bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
					ms.Seek(0, SeekOrigin.Begin);
					return ms;
				}
			}
		}


		#region Custom exceptions class

		public class IconNotFoundException : Exception {
			public IconNotFoundException(string fileName, int index)
				: base(string.Format("Icon with Id = {0} wasn't found in file {1}", index, fileName)) {
			}
		}

		public class UnableToExtractIconsException : Exception {
			public UnableToExtractIconsException(string fileName, int firstIconIndex, int iconCount)
				: base(string.Format("Tryed to extract {2} icons starting from the one with id {1} from the \"{0}\" file but failed", fileName, firstIconIndex, iconCount)) {
			}
		}

		#endregion

		#region DllImports

		struct SHFILEINFO {
			public IntPtr hIcon;

			public IntPtr iIcon;

			public uint dwAttributes;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		};

		[Flags]
		enum FileInfoFlags : int {
			
			SHGFI_ICON = 0x000000100,
			
			SHGFI_USEFILEATTRIBUTES = 0x000000010
		}

		/// <summary>
		///     Creates an array of handles to large or small icons extracted from
		///     the specified executable file, dynamic-link library (DLL), or icon
		///     file. 
		/// </summary>
		/// <param name="lpszFile">
		///     Name of an executable file, DLL, or icon file from which icons will
		///     be extracted.
		/// </param>
		/// <param name="nIconIndex">
		///     <para>
		///         Specifies the zero-based index of the first icon to extract. For
		///         example, if this value is zero, the function extracts the first
		///         icon in the specified file.
		///     </para>
		///     <para>
		///         If this value is �1 and <paramref name="phiconLarge"/> and
		///         <paramref name="phiconSmall"/> are both NULL, the function returns
		///         the total number of icons in the specified file. If the file is an
		///         executable file or DLL, the return value is the number of
		///         RT_GROUP_ICON resources. If the file is an .ico file, the return
		///         value is 1. 
		///     </para>
		///     <para>
		///         Windows 95/98/Me, Windows NT 4.0 and later: If this value is a 
		///         negative number and either <paramref name="phiconLarge"/> or 
		///         <paramref name="phiconSmall"/> is not NULL, the function begins by
		///         extracting the icon whose resource identifier is equal to the
		///         absolute value of <paramref name="nIconIndex"/>. For example, use -3
		///         to extract the icon whose resource identifier is 3. 
		///     </para>
		/// </param>
		/// <param name="phIconLarge">
		///     An array of icon handles that receives handles to the large icons
		///     extracted from the file. If this parameter is NULL, no large icons
		///     are extracted from the file.
		/// </param>
		/// <param name="phIconSmall">
		///     An array of icon handles that receives handles to the small icons
		///     extracted from the file. If this parameter is NULL, no small icons
		///     are extracted from the file. 
		/// </param>
		/// <param name="nIcons">
		///     Specifies the number of icons to extract from the file. 
		/// </param>
		/// <returns>
		///     If the <paramref name="nIconIndex"/> parameter is -1, the
		///     <paramref name="phIconLarge"/> parameter is NULL, and the
		///     <paramref name="phiconSmall"/> parameter is NULL, then the return
		///     value is the number of icons contained in the specified file.
		///     Otherwise, the return value is the number of icons successfully
		///     extracted from the file. 
		/// </returns>
		[DllImport("Shell32", CharSet = CharSet.Auto)]
		extern static int ExtractIconEx(
			[MarshalAs(UnmanagedType.LPTStr)] 
		string lpszFile,
			int nIconIndex,
			IntPtr[] phIconLarge,
			IntPtr[] phIconSmall,
			int nIcons);

		[DllImport("Shell32", CharSet = CharSet.Auto)]
		extern static IntPtr SHGetFileInfo(
			string pszPath,
			int dwFileAttributes,
			out SHFILEINFO psfi,
			int cbFileInfo,
			FileInfoFlags uFlags);

		#endregion

		#region ExtractIcon-like functions

		public static void ExtractEx(string fileName, List<Icon> largeIcons,
			List<Icon> smallIcons, int firstIconIndex, int iconCount) {
			
			IntPtr[] smallIconsPtrs = null;
			IntPtr[] largeIconsPtrs = null;

			if (smallIcons != null) {
				smallIconsPtrs = new IntPtr[iconCount];
			}
			if (largeIcons != null) {
				largeIconsPtrs = new IntPtr[iconCount];
			}

			int apiResult = ExtractIconEx(fileName, firstIconIndex, largeIconsPtrs, smallIconsPtrs, iconCount);
			if (apiResult != iconCount) {
				throw new UnableToExtractIconsException(fileName, firstIconIndex, iconCount);
			}


			if (smallIcons != null) {
				smallIcons.Clear();
				foreach (IntPtr actualIconPtr in smallIconsPtrs) {
					smallIcons.Add(Icon.FromHandle(actualIconPtr));
				}
			}
			if (largeIcons != null) {
				largeIcons.Clear();
				foreach (IntPtr actualIconPtr in largeIconsPtrs) {
					largeIcons.Add(Icon.FromHandle(actualIconPtr));
				}
			}
		}

		public static List<Icon> ExtractEx(string fileName, FileIconSize size,
			int firstIconIndex, int iconCount) {
			List<Icon> iconList = new List<Icon>();

			switch (size) {
				case FileIconSize.Large:
					ExtractEx(fileName, iconList, null, firstIconIndex, iconCount);
					break;

				case FileIconSize.Small:
					ExtractEx(fileName, null, iconList, firstIconIndex, iconCount);
					break;

				default:
					throw new ArgumentOutOfRangeException("size");
			}

			return iconList;
		}

		public static void Extract(string fileName, List<Icon> largeIcons, List<Icon> smallIcons) {
			int iconCount = GetIconsCountInFile(fileName);
			ExtractEx(fileName, largeIcons, smallIcons, 0, iconCount);
		}

		public static List<Icon> Extract(string fileName, FileIconSize size) {
			int iconCount = GetIconsCountInFile(fileName);
			return ExtractEx(fileName, size, 0, iconCount);
		}

		public static Icon ExtractOne(string fileName, int index, FileIconSize size) {
			try {
				List<Icon> iconList = ExtractEx(fileName, size, index, 1);
				return iconList[0];
			} catch (UnableToExtractIconsException) {
				throw new IconNotFoundException(fileName, index);
			}
		}

		public static void ExtractOne(string fileName, int index,
			out Icon largeIcon, out Icon smallIcon) {
			List<Icon> smallIconList = new List<Icon>();
			List<Icon> largeIconList = new List<Icon>();
			try {
				ExtractEx(fileName, largeIconList, smallIconList, index, 1);
				largeIcon = largeIconList[0];
				smallIcon = smallIconList[0];
			} catch (UnableToExtractIconsException) {
				throw new IconNotFoundException(fileName, index);
			}
		}

		#endregion



		/// <summary>
		/// Get the number of icons in the specified file.
		/// </summary>
		/// <param name="fileName">Full path of the file to look for.</param>
		/// <returns></returns>
		static int GetIconsCountInFile(string fileName) {
			return ExtractIconEx(fileName, -1, null, null, 0);
		}

		public static Icon IconFromExtension(string extension,
												FileIconSize size) {
			
			if (extension[0] != '.') extension = '.' + extension;
			extension = extension.ToLower();

			RegistryKey Root = Registry.ClassesRoot;
			RegistryKey ExtensionKey = Root.OpenSubKey(extension);
			ExtensionKey.GetValueNames();
			RegistryKey ApplicationKey =
				Root.OpenSubKey(ExtensionKey.GetValue("").ToString());

			string IconLocation =
				ApplicationKey.OpenSubKey("DefaultIcon").GetValue("").ToString();
			string[] IconPath = IconLocation.Split(',');

			if (IconPath[1] == null) IconPath[1] = "0";
			IntPtr[] Large = new IntPtr[1], Small = new IntPtr[1];

			ExtractIconEx(IconPath[0],
				Convert.ToInt16(IconPath[1]), Large, Small, 1);
			return size == FileIconSize.Large ?
				Icon.FromHandle(Large[0]) : Icon.FromHandle(Small[0]);
		}

		/// <summary>
		/// Parse strings in registry who contains the name of the icon and
		/// the index of the icon an return both parts.
		/// </summary>
		/// <param name="regString">The full string in the form "path,index" as found in registry.</param>
		/// <param name="fileName">The "path" part of the string.</param>
		/// <param name="index">The "index" part of the string.</param>
		public static void ExtractInformationsFromRegistryString(
			string regString, out string fileName, out int index) {
			if (regString == null) {
				throw new ArgumentNullException("regString");
			}
			if (regString.Length == 0) {
				throw new ArgumentException("The string should not be empty.", "regString");
			}

			index = 0;
			string[] strArr = regString.Replace("\"", "").Split(',');
			fileName = strArr[0].Trim();
			if (strArr.Length > 1) {
				int.TryParse(strArr[1].Trim(), out index);
			}
		}

	}

}