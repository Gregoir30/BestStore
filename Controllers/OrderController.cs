using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BestStore.Extensions;

namespace BestStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        // Affiche la page de confirmation
        public IActionResult Confirm()
        {
            // Ici, on pourrait charger les éléments du panier depuis la session
            // et afficher le total à payer.
            return View();
        }

        private readonly Services.PdfService _pdfService;

        public OrderController(Services.PdfService pdfService)
        {
            _pdfService = pdfService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit()
        {
            // Enregistrement de la commande à la base
            // (non implémenté ici, mais à faire après)
            // Vider le panier...

            // For demonstration, generate PDF from cart in session
            var cart = HttpContext.Session.GetObject<Models.Cart>("Cart");
            if (cart == null || cart.Items.Count == 0)
            {
                TempData["Error"] = "Votre panier est vide.";
                return RedirectToAction("Index", "Cart");
            }

            // Generate PDF and store in TempData or session for download
            // Convert cart to HTML string for PDF generation
            var html = $@"
                <h1>Commande</h1>
                <p>Date: {System.DateTime.Now}</p>
                <ul>
                    {string.Join("", cart.Items.Select(item => $"<li>{item.Name} x {item.Quantity} - {item.Price:C}</li>"))}
                </ul>
                <p><strong>Total:</strong> {cart.GetTotal():C}</p>
            ";

            var pdfBytes = _pdfService.GenerateCartPdf(html);
            HttpContext.Session.Set("OrderPdf", pdfBytes);

            TempData["Success"] = "Commande validée avec succès !";

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult DownloadPdf()
        {
            var pdfBytes = HttpContext.Session.Get("OrderPdf");
            if (pdfBytes == null)
            {
                TempData["Error"] = "Aucun PDF disponible pour téléchargement.";
                return RedirectToAction("Index", "Home");
            }
            return File(pdfBytes, "application/pdf", "Commande.pdf");
        }
    }
}
