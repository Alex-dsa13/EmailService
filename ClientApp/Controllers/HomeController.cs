using ClientApp.Models;
using ClientApp.RabbitMq;
using Microsoft.AspNetCore.Mvc;

namespace ClientApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRabbitMqService _mqService;

        public HomeController(ILogger<HomeController> logger, IRabbitMqService mqService)
        {
            _logger = logger;
            _mqService = mqService;
        }

        [HttpPost]
        public IActionResult Send(EmailData data)
        {
            string response;

            if (ModelState.IsValid)
            {
                var email = data.Email;
                var message = data.Message;

                var allInfo = email + "/" + message;
                _mqService.RegisterRpcClient();
                var task = Task.Run(() => _mqService.CallAsync(allInfo));
                task.Wait();

                response = _mqService.GetResponse();

                _mqService.Close();

                if (response == "Message sended!")
                {
                    ViewBag.SendStatus = true;
                    ViewBag.Message = response;
                }
                else
                {
                    ViewBag.SendStatus = false;
                    ViewBag.Message = response;
                }

                return View();

            }
            else
            {
                return View("Index", data);
            }
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}