using DeveloperUtilsLibrary;
using ProcessiingContext.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFlex.DOCs.Common;
using TFlex.DOCs.Model;
using TFlex.DOCs.Model.Macros;
using TFlex.DOCs.Model.References;
using TFlex.DOCs.Model.References.Files;
using TFlex.DOCs.Model.References.Nomenclature;
using TFlex.DOCs.Resources.Strings;

namespace ProcessiingContext.Handler
{
    public class ProcessingNotice
    {
        private Notice notice;
        private ServerConnection connection;
        private NomenclatureHandler nomenclatureHandler;
        String _tempFolder = Path.Combine(Path.GetTempPath(), "Temp DOCs", "ReportNoticeInOtherContext");
        //String extensionDoc = "txt";

        ConfigurationSettings config;

        private FileHandler fileHandler;
        public ProcessingNotice(ReferenceObject _notice, ServerConnection connection, ConfigurationSettings configurationSettings)
        {
            this.notice = new Notice(_notice, connection, configurationSettings);
            this.connection = connection;
            this.nomenclatureHandler = new NomenclatureHandler(connection);

            config = new ConfigurationSettings(connection)
            {
                ApplyDesignContext = true,
                ShowDeletedInDesignContextLinks = true
            };
        }

        public void SetTempFolder(string tempPath)
        {
            this._tempFolder = tempPath;
        }
        ///// <summary>
        ///// Проверяет возможность переноса текущего ИИ в заданный контекст
        ///// </summary>
        ///// <param name="targetContext">Целевой контекст</param>
        ///// <returns>true если можно перенести</returns>
        //public bool IsEnableMoveInContext(DesignContextObject targetContext)
        //{
        //    var sourceLinks = notice.GetAllSourceHierarchyLinks();
        //    foreach (var sourceLink in sourceLinks)
        //    {
        //        var findSourceLink = nomenclatureHandler.FindComplexHierarhyLink(sourceLink, targetContext);
        //        if (findSourceLink is null)
        //        {
        //            return false;
        //        }
        //    }
        //    return true;
        //}

        /// <summary>
        /// Возвращает текстовое сообщение с информацией о возможности переноса в указанный контекст
        /// </summary>
        /// <param name="targetContext">целевой контекст</param>
        /// <returns>отчет</returns>
        //public String GetReportMoveInContext(DesignContextObject targetContext)
        //{
        //    StringBuilder stringBuilder = new StringBuilder();
        //    var sourceLinks = notice.GetAllSourceHierarchyLinks();
        //    foreach (var sourceLink in sourceLinks)
        //    {
        //        var findSourceLink = nomenclatureHandler.FindComplexHierarhyLink(sourceLink, targetContext);
        //        if (findSourceLink is null)
        //        {
        //            stringBuilder.AppendLine($"Не было найдено подключение c id: {sourceLink.Id} в контексте {targetContext} между {sourceLink.ParentObject} и {sourceLink.ChildObject}\n");
        //        }
        //    }
        //    if (stringBuilder.Length == 0)
        //    {
        //        stringBuilder.AppendLine($"ИИ можно перенести в {targetContext}");
        //    }
        //    return stringBuilder.ToString();
        //}

        
        /// <summary>
        /// Формирует отчет об ии в заданном контексте
        /// </summary>
        /// <param name="targetContext"></param>
        /// <param name="fileName"></param>
        /// <param name="parentFoler"></param>
        /// <param name="extensionDoc"></param>
        /// <param name="isNewFile"></param>
        /// <returns></returns>
        public FileObject GenerateReportInContext(DesignContextObject targetContext, String fileName, String parentFoler, String extensionDoc, bool isNewFile)
        {

            fileHandler = new FileHandler(connection, isNewFile);

            var referenceNotice = notice.NoticeObject.Reference;
            string tempFilePath = Path.Combine(_tempFolder, String.Format("{0}.{1}", Guid.NewGuid(), extensionDoc));


            using (var ws = new StreamWriter(tempFilePath))
            {
                config.DesignContext = targetContext;
                using (referenceNotice.ChangeAndHoldConfigurationSettings(config))
                {
                    referenceNotice.Refresh();
                    notice.NoticeObject.Reload();

                    var noticeChanges = new Notice(notice.NoticeObject, this.connection, config);
                    ws.Write(noticeChanges);
                    ws.WriteLine();
                    ws.Write("target config");
                    ws.WriteLine(config);
                    ws.WriteLine();
                }
            }

            var file = fileHandler.UploadFile(tempFilePath, parentFoler, fileName, extensionDoc);
            fileHandler.ClearTemp(_tempFolder);
            return file;
        }
    }
}
