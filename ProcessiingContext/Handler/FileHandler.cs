using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.Desktop;
using TFlex.DOCs.Model.Macros;
using TFlex.DOCs.Model.References.Files;
using TFlex.DOCs.Resources.Strings;

namespace ProcessiingContext.Handler
{
    public class FileHandler
    {



        private bool isGenerateNewFile;

        FileReference fileReference;
        public FileHandler(ServerConnection connection, bool isGenerateNewFile) {
            this.fileReference = new FileReference(connection)
            {
                LoadSettings = { LoadDeleted = true }
            };
            this.isGenerateNewFile = isGenerateNewFile;
        }

        /// <summary>
        /// Загрузка файла в T-FLEX DOCs
        /// </summary>
        /// <param name="localFilePath">путь до локального файла</param>
        /// <param name="parentFolderPath">относительный путь до папки  T-FLEX DOCs</param>
        /// <param name="fileName">имя сохраненного файла</param>
        /// <param name="_extensionDoc">расширение файла</param>
        /// <returns></returns>
        /// <exception cref="MacroException"></exception>
        public FileObject UploadFile(string localFilePath, string parentFolderPath, string fileName, string _extensionDoc)
        {
            try
            {
                var parentFolder = (FolderObject)fileReference.FindByRelativePath(parentFolderPath);
                if (parentFolder == null)
                    throw new MacroException(String.Format("Не найдена родительская папка с именем '{0}'", parentFolderPath));

                parentFolder.Children.Load();

                var uploadingFileName = String.Format("{0}.{1}", fileName, _extensionDoc);
                var exportedFile = parentFolder.Children.AsList
                    .FirstOrDefault(child => child.IsFile && child.Name.Value == uploadingFileName) as FileObject;

                if (exportedFile is null)
                {
                    var fileType = GetFileType(_extensionDoc);
                    exportedFile = parentFolder.CreateFile(
                        localFilePath,
                        String.Empty,
                        uploadingFileName,
                        fileType);
                }
                else
                {
                    if (isGenerateNewFile)
                    {
                        var fileType = GetFileType(_extensionDoc);
                        exportedFile = parentFolder.CreateFile(
                            localFilePath,
                            String.Empty,
                            GetUniqueExportedFileName(uploadingFileName, parentFolder, fileName, _extensionDoc),
                            fileType);
                    }
                    else
                    {
                        if (!exportedFile.IsCheckedOutByCurrentUser)
                            Desktop.CheckOut(exportedFile, false);

                        File.Copy(localFilePath, exportedFile.LocalPath, true);
                    }
                }
                Desktop.CheckIn(exportedFile, String.Format(
                    "Экспорт файла:{0}'{1}'{0}в формат '{3}':{0}'{2}'",
                    Environment.NewLine, exportedFile.Path, exportedFile.Path, _extensionDoc), false);
                return exportedFile;
            }
            catch (SystemException e)
            {
                string exceptionMessage = String.Format(
                    "Ошибка загрузки файла на сервер.{0}" +
                    "При операции загрузки файла на сервер произошли следующие ошибки:{0}{1}",
                    Environment.NewLine, e.Message);
                throw new MacroException(exceptionMessage, e);
            }
        }
        private FileType GetFileType(String _extensionDoc)
        {
            var fileType = fileReference.Classes.GetFileTypeByExtension(_extensionDoc);
            if (fileType is null)
            {
                string typeName = String.Format(Texts.FileNameWithExtension, _extensionDoc.ToUpper());
                fileType = fileReference.Classes.CreateFileType(typeName, String.Empty, _extensionDoc);
            }

            return fileType;
        }

        private string GetUniqueExportedFileName(string exportedFileName, FolderObject parentFolder, string fileName, string extensionDoc)
        {
            var filesName = parentFolder.Children.AsList
                .Where(child => child.IsFile)
                .Select(file => file.Name.Value)
                .ToArray();

            var filesNameSet = new HashSet<string>(filesName);

            var counter = 1;

            while (filesNameSet.Contains(exportedFileName))
            {
                exportedFileName = String.Format("{0}_{1}.{2}", fileName, counter, extensionDoc);
                counter++;
            }

            return exportedFileName;
        }

        /// <summary>
        /// Удаление временной папки
        /// </summary>
        public void ClearTemp(string tempFolder)
        {
            if (Directory.Exists(tempFolder))
            {
                foreach (string filePath in Directory.GetFiles(tempFolder))
                    DeleteFile(filePath);
            }
        }

        /// <summary>
        /// Удаление файла
        /// </summary>
        /// <param name="path">путь к файлу</param>
        private void DeleteFile(string path)
        {
            if (!File.Exists(path))
                return;

            // Получаем атрибуты файла
            var fileAttribute = File.GetAttributes(path);

            if ((fileAttribute & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                // Удаляем атрибут 'Только для чтения'
                var removeAttributes = RemoveAttribute(fileAttribute, FileAttributes.ReadOnly);
                File.SetAttributes(path, removeAttributes);
            }

            File.Delete(path);
        }

        private FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }
    }
}
