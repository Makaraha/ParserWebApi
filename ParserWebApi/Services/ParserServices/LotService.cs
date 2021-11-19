using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParserWebApi.Services.ParserServices
{
    public class LotService
    {
        private FieldService fieldService { get; set; }
        private LinkService linkService { get; set; }


        private readonly HashSet<string> blocksBlackList = new HashSet<string> { "Заказчики", "Документы лота" };

        public LotService(FieldService fieldService, LinkService linkService)
        {
            this.fieldService = fieldService;
            this.linkService = linkService;
        }
        // Парсинг страницы закупок


        public async Task<List<Lot>> ParseAsync(HtmlDocument procurementDoc, int lotDiv)
        {
            var links = await linkService.CollectLotsAsync(procurementDoc, lotDiv);
            var lots = new List<Lot>();

            foreach (var link in links)
            {
                var lotDoc = linkService.Load(link);
                var lot = await ParseLotAsync(lotDoc);
                lots.Add(lot);
            }
            return lots;
        }

        private async Task<Lot> ParseLotAsync(HtmlDocument doc)
        {
            int blockId = 1;
            var lot = new Lot();

            while (true)
            {
                var blockName = fieldService.GetTextFromXPath($"html/body/form/div[4]/div[5]/div[2]/div[{blockId}]/div/a[2]",
                    doc);
                if (blockName is null)
                    break;

                if (blockName == "Заказчики")
                    lot.Customers = await ParseCustomersAsync(doc);


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
                    var fieldName = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[{blockId}]/table/tr[{fieldId}]/td[1]",
                        doc);
                    //fieldName = fieldService.BringToStandart(fieldName);

                    if (fieldName is null)
                        break;

                    var fieldValue = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[{blockId}]/table/tr[{fieldId}]/td[2]",
                        doc);
                    fieldValue = fieldService.BringToStandart(fieldValue);

                    var field = new Field();
                    field.Name = fieldName;
                    field.Value = fieldValue;

                    block.Fields.Add(field);
                    fieldId++;
                }

                lot.Blocks.Add(block);
                blockId++;
            }
            return await Task.FromResult(lot);
        }

        private async Task<List<Customer>> ParseCustomersAsync(HtmlDocument lotDoc)
        {
            var customers = new List<Customer>();
            int customerNumber = 1;
            while (true)
            {
                
                var customer = await ParseCustomerAsync(lotDoc, customerNumber);

                if (customer is null)
                    break;

                customers.Add(customer);
                customerNumber++;
            }
            return customers;
        }

        private async Task<Customer> ParseCustomerAsync(HtmlDocument lotDoc, int customerNumber)
        {
            var customer = new Customer();
            int i = 1;

            while (true)
            {
                var fieldName = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[2]/div[2]/table[{customerNumber}]/tr[{i}]/td[1]",
                    lotDoc);


                if (fieldName is null)
                    if (i == 1)
                        return null;
                    else
                        break;


                var fieldValue = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[2]/div[2]/table[{customerNumber}]/tr[{i}]/td[2]",
                    lotDoc);

                fieldValue = fieldService.BringToStandart(fieldValue);

                customer.Fields.Add(new Field { Name = fieldName, Value = fieldValue });
                i++;
            }
            customer.GPZs = CollectGPZ(lotDoc, customerNumber);
            return await Task.FromResult(customer);
        }

        private List<GPZ> CollectGPZ(HtmlDocument lotDoc, int customerNumber)
        {
            ///html/body/form/div[4]/div[5]/div[2]/div[2]/div[2]/div[{customerNumber}]/table/tbody/tr[{gpzNumber}]/td[{i}]/p

            var GPZs = new List<GPZ>();
            int gpzCount = 1;

            while (true)
            {
                var gpzNumber = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[2]/div[2]/div[{customerNumber}]/table/tbody/tr[{gpzCount}]/td[{1}]/p",
                    lotDoc);
                if (gpzNumber is null)
                    return GPZs;

                var gpz = new GPZ();
                gpz.Number = gpzNumber;
                gpz.Name = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[2]/div[2]/div[{customerNumber}]/table/tbody/tr[{gpzCount}]/td[{2}]/p",
                    lotDoc);
                gpz.Count = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[2]/div[2]/div[{customerNumber}]/table/tbody/tr[{gpzCount}]/td[{3}]/p",
                    lotDoc);
                gpz.Country = fieldService.GetTextFromXPath($"/html/body/form/div[4]/div[5]/div[2]/div[2]/div[2]/div[{customerNumber}]/table/tbody/tr[{gpzCount}]/td[{4}]/p",
                    lotDoc);
                GPZs.Add(gpz);
                gpzCount++;
            }


        }
    }
}