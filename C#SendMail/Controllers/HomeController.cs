using C_SendMail.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using C_SendMail.Models;
using System.Threading.Tasks;

namespace C_SendMail.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    [HttpPost]
    public async Task<IActionResult> UploadCsv(IFormFile CsvFile, string Subject, string Body)
    {
        if (CsvFile == null || CsvFile.Length == 0)
        {
            ModelState.AddModelError("", "Por favor, selecione um arquivo CSV.");
            return View("Index");
        }

        var clients = new List<Client>();

        // Processar o arquivo CSV
        using (var reader = new StreamReader(CsvFile.OpenReadStream()))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            clients = csv.GetRecords<Client>().ToList();
        }

        // Enviar e-mails para todos os clientes
        foreach (var client in clients)
        {
            await SendEmail(client.Email, Subject, Body.Replace("{Nome}", client.Nome));
        }

        return View("Success");
    }

    private async Task SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            var mail = new MailMessage
            {
                From = new MailAddress("seuemail@empresa.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mail.To.Add(toEmail);

            using (var smtp = new SmtpClient("smtp.seuservidor.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("seuemail@empresa.com", "suasenha"),
                EnableSsl = true
            })
            {
                await smtp.SendMailAsync(mail);
            }
        }
        catch (Exception ex)
        {
            // Tratar erro de envio de e-mail
            Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
        }
    }

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Utilizacao()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}