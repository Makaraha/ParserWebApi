using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParserWebApi.Services.ParserServices;
using System;
using System.Threading.Tasks;

namespace ParserWebApi.Controllers
{
    [ApiController]
    [Route("Parser")]
    public class ParserController : ControllerBase
    {
        private ParserService parserService { get; set; }
        public ParserController(ParserService parserService)
        {
            this.parserService = parserService;
        }

        [Route("start")]
        [HttpPost]
        public string Start()
        {
            Task.Run(async () => await parserService.StartAsync());
            return "Парсер успешно запущен.";
        }

        [Route("update")]
        [HttpPost]
        public string Update()
        {
            Task.Run(async() => await parserService.UpdateAsync());
            return "Обновление данных запущено.";
        }
    }
}
