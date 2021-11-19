using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ParserWebApi.Services
{
    // Сервис для работы с ссылками
    public class LinkService
    {
        private FieldService fieldService { get; set; }
        private ILogger logger { get; set; }
        public LinkService(FieldService fieldService, ILogger<LinkService> logger)
        {
            this.fieldService = fieldService;
            this.logger = logger;
        }

        private string linkBase { get; set; } = "https://zakupki.rosatom.ru/";
        private const int LOAD_TIMEOUT = 20;

        /// <summary>
        /// Собирает все ссылки на закупки
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public async Task<List<string>> CollectProcAsync(HtmlDocument doc)
        {
            var result = new List<string>();
            int i = 1;

            while (true)
            {
                // Для перехода к следующей закупки нужно увеличить i на (1 + кол-во лотов)
                var node = doc.DocumentNode
                    .SelectSingleNode($"/html/body/form/div[4]/div[5]/div[2]/div[3]/table/tbody/tr[{i}]/td[3]/p[2]/a");

                var status = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[3]/table/tbody/tr[{i}]/td[8]/p",
                    doc);

                if (status == "Черновик")
                    continue;

                if (node is null)
                    break;

                var lotCount = doc.DocumentNode
                    .SelectSingleNode($"/html/body/form/div[4]/div[5]/div[2]/div[3]/table/tbody/tr[{i}]/td[4]/p[2]")
                    .ChildNodes[1].InnerText;

                i += 1 + int.Parse(lotCount);
                var link = linkBase + node.Attributes["href"].Value;
                result.Add(link);
            }

            if (result.Count == 0)
                throw new Exception("Collecting links returns 0");
            return await Task.FromResult(result);
        }


        public async Task<List<string>> CollectLotsAsync(HtmlDocument procurementDoc, int lotDiv)
        {
            var result = new List<string>();
            int i = 1;

            while (true)
            {
                var node = procurementDoc.DocumentNode
                    .SelectSingleNode($"/html/body/form/div[4]/div[5]/div[2]/div[5]/div[{lotDiv}]/div[2]/table/tbody/tr[{i}]/td[1]/p/a");
                var number =
                    fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[5]/div[{lotDiv}]/div[2]/table/tbody/tr[{i}]/td[1]/p/a/b", procurementDoc);

                if (node is null)
                    break;

                var link = linkBase + node.Attributes["href"].Value;

                result.Add(link);

                i++;
            }
            return await Task.FromResult(result);
        }
        


        public HtmlDocument Load(string link)
        {
            var web = new HtmlWeb();
            while (true)
            {
                try
                {
                    var task = Task.Run(() => web.Load(link));
                    if (task.Wait(TimeSpan.FromSeconds(LOAD_TIMEOUT)))
                        return task.Result;
                    else
                        throw new Exception($"Loading timeout. Link = {link}");
                }
                catch(Exception ex)
                {
                    logger.LogWarning($"Ошибка во время загрузки страницы: {ex.Message}");
                    Thread.Sleep(1000);
                }

            }
        }

        public HtmlDocument LoadPage(int i)
        {
            var link = $"https://zakupki.rosatom.ru/Web.aspx?node=currentorders&page={i}";
            return Load(link);
        }
    }
}
