using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParserWebApi.Services.ParserServices
{
    public class ParserService
    {
        private LinkService linkService { get; set; }
        private ProcurementService procurementService { get; set; }
        private RssService rssService { get; set; }
        private ILogger logger { get; set; }
        public ParserService(LinkService linkService, ProcurementService procurementService, ILogger<ParserService> logger,
            RssService rssService)
        {
            this.linkService = linkService;
            this.procurementService = procurementService;
            this.logger = logger;
            this.rssService = rssService;
        }


        public async Task StartAsync()
        {
            int page = 1;
            logger.LogInformation($"Парсер запущен");

            while (true)
            {
                logger.LogInformation($"Загрузка страницы {page}");
                var doc = linkService.LoadPage(page);
                logger.LogInformation($"Страница загружена. Собираю ссылки на закупки");
                var procurementsLinks = await linkService.CollectProcAsync(doc);
                logger.LogInformation($"Собрано {procurementsLinks.Count} ссылок");

                logger.LogInformation($"Начинаю парсинг закупок");
                Parallel.ForEach(procurementsLinks, async link => 
                //foreach(var link in procurementsLinks)
                {
                    try
                    {
                        var procurementDoc = linkService.Load(link);
                        var procurement = await procurementService.ParseAsync(procurementDoc);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception($"Ошибка во время парсинга закупки {link}: {ex.Message}");
                    }
                });
                logger.LogInformation($"Страница {page} успешно спаршена");
                page++;
            }
        }

        public async Task UpdateAsync()
        {
            var procurements = await rssService.ExtractProcurementsAsync();
        }
    }
}