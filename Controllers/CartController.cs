using BestStore.Models;
using BestStore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BestStore.Controllers
{
    public class CartController : Controller
    {
        private const string CartKey = "Cart";

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartKey) ?? new Cart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult Add(int id, string name, decimal price)
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartKey) ?? new Cart();

            cart.AddItem(new CartItem
            {
                ProductId = id,
                Name = name,
                Price = price,
                Quantity = 1
            });

            HttpContext.Session.SetObject(CartKey, cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObject<Cart>(CartKey);
            if (cart != null)
            {
                cart.RemoveItem(id);
                HttpContext.Session.SetObject(CartKey, cart);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartKey);
            return RedirectToAction("Index");
        }
    }
}
