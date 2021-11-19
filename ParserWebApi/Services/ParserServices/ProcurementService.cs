using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParserWebApi.Services.ParserServices
{
    public class ProcurementService
    {
        private FieldService fieldService { get; set; }
        private LinkService linkService { get; set; }
        private LotService lotService { get; set; }
        private ILogger logger { get; set; }
        

        private readonly HashSet<string> blocksBlackList = new HashSet<string> { "Лоты", "Документы" };

        public ProcurementService(FieldService fieldService, LinkService linkService, LotService lotService, ILogger<ProcurementService> logger)
        {
            this.fieldService = fieldService;
            this.linkService = linkService;
            this.lotService = lotService;
            this.logger = logger;
        }

        // Парсинг страницы закупок
        public async Task<Procurement> ParseAsync(HtmlDocument doc)
        {
            var procurement = new Procurement();
            int blockId = 1;
            while (true)
            {

                var blockName = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[5]/div[{blockId}]/div/a[2]",
                    doc);
                if (blockName is null)
                    break;


                if (blockName == "Лоты")
                {
                    var lots = await lotService.ParseAsync(doc, blockId);
                    procurement.Lots = lots;
                }

                if (blocksBlackList.Contains(blockName))
                {
                    blockId++;
                    continue;
                } 

                var block = new Block();
                block.Name = blockName;



                int fieldId = 1;
                while (true)
                {
                    var fieldName = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[5]/div[{blockId}]/table/tr[{fieldId}]/td[1]",
                        doc);
                    //fieldName = fieldService.BringToStandart(fieldName);

                    if (fieldName is null)
                            break;

                    var fieldValue = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[5]/div[{blockId}]/table/tr[{fieldId}]/td[2]",
                        doc);
                    fieldValue = fieldService.BringToStandart(fieldValue);

                    block.Fields.Add(new Field() { Name = fieldName, Value = fieldValue });
                    fieldId++;
                }
               
                procurement.Blocks.Add(block);
                blockId++;
            }
            return await Task.FromResult(procurement);
        }
    }
}