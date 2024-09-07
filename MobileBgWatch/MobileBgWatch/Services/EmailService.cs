using Microsoft.Extensions.Options;
using MobileBgWatch.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;

namespace MobileBgWatch.Services
{
    public class EmailService : IEmailService
    {
        private readonly SendGridClient _sendGridClient;
        private readonly ILogger<EmailService> _logger;
        private readonly string _senderEmail;

        public EmailService(IOptions<SendGridOptions> options, ILogger<EmailService> logger)
        {
            _sendGridClient = new SendGridClient(options.Value.ApiKey);
            _senderEmail = options.Value.SenderEmail;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, List<List<Vehicle>> newVehicleAds)
        {
            var vehicles = newVehicleAds.SelectMany(list => list).ToList();
            var htmlContent = GenerateEmailHtml(vehicles);

            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_senderEmail),
                Subject = subject,
                HtmlContent = htmlContent
            };

            msg.AddTo(new EmailAddress(email));

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Email send to {email}");
            }
            else
            {
                _logger.LogError($"Failed to send email to {email}. Status code: {response.StatusCode}");
            }
        }

        private string GenerateEmailHtml(List<Vehicle> vehicles)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif; }");
            sb.AppendLine("table { width: 100%; max-width: 600px; margin: auto; background-color: #ffffff; border: 1px solid #ddd; }");
            sb.AppendLine("td { padding: 20px; }");
            sb.AppendLine(".header { background-color: #212121; color: #ffffff; text-align: center; }");
            sb.AppendLine(".content { padding: 20px; }");
            sb.AppendLine(".vehicle { border: 1px solid #ddd; margin: 10px 0; padding: 10px; background-color: #ffffff; }");
            sb.AppendLine(".vehicle img { width: 100%; height: auto; }");
            sb.AppendLine(".vehicle h2 { font-size: 18px; color: #333333; }");
            sb.AppendLine(".vehicle p { font-size: 14px; color: #666666; }");
            sb.AppendLine(".footer { background-color: #f4f4f4; text-align: center; color: #999999; font-size: 12px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><td class='header'><h1>New Vehicle Ads</h1></td></tr>");
            sb.AppendLine("<tr><td class='content'>");

            foreach (var vehicle in vehicles)
            {
                sb.AppendLine("<div class='vehicle'>");
                sb.AppendLine($"<img src='{vehicle.ImageUrls.FirstOrDefault()}' alt='Vehicle Image'>");
                sb.AppendLine($"<h2>{vehicle.Name}</h2>");
                sb.AppendLine($"<p>Current Price: {vehicle.CurrentPrice.Price.ToString("N0")} {vehicle.CurrentPrice.Currency}</p>");
                sb.AppendLine($"<p>Previous Price: {vehicle.PreviousPrice.Price.ToString("N0")} {vehicle.PreviousPrice.Currency}</p>");
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</td></tr>");
            sb.AppendLine("<tr><td class='footer'><p>&copy; 2024 YourAppName. All rights reserved.</p></td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}
