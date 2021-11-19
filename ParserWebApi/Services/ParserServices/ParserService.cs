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
            logger.LogInformation($"������ �������");

            while (true)
            {
                logger.LogInformation($"�������� �������� {page}");
                var doc = linkService.LoadPage(page);
                logger.LogInformation($"�������� ���������. ������� ������ �� �������");
                var procurementsLinks = await linkService.CollectProcAsync(doc);
                logger.LogInformation($"������� {procurementsLinks.Count} ������");

                logger.LogInformation($"������� ������� �������");
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
                        throw new Exception($"������ �� ����� �������� ������� {link}: {ex.Message}");
                    }
                });
                logger.LogInformation($"�������� {page} ������� ��������");
                page++;
            }
        }

        public async Task UpdateAsync()
        {
            var procurements = await rssService.ExtractProcurementsAsync();
        }
    }
}