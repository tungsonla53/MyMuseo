using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMuseo.Models;
using MyMuseo.DataService;
using Microsoft.AspNet.Identity;

namespace MyMuseo.Controllers
{
    public class ShoppingCartController : Controller
    {
        private CollectiblesRespository _collectiblesRespository;
        public ShoppingCartController()
        {
            _collectiblesRespository = new CollectiblesRespository();
        }
        public ActionResult Index()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
            };
            return View(viewModel);
        }
        public ActionResult AddToCart(int id)
        {
            var addedCollectibe = _collectiblesRespository.GetCollectible(id);
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.AddToCart(addedCollectibe);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateCartCount(int id, int cartCount)
        {
            ShoppingCartRemoveViewModel results = null;
            try
            {
                var cart = ShoppingCart.GetCart(this.HttpContext);

                // Todo
                ShoppingCartRepository storeDB = new ShoppingCartRepository();
                string collectibleTitle = storeDB.Carts()
                    .Single(item => item.RecordId == id).Title;

                // Update the cart count 
                int itemCount = cart.UpdateCartCount(id, cartCount);

                //Prepare messages
                string msg = "The quantity of " + Server.HtmlEncode(collectibleTitle) +
                        " has been refreshed in your shopping cart.";
                if (itemCount == 0) msg = Server.HtmlEncode(collectibleTitle) +
                        " has been removed from your shopping cart.";
                //
                // Display the confirmation message 
                results = new ShoppingCartRemoveViewModel
                {
                    Message = msg,
                    CartTotal = cart.GetTotal(),
                    CartCount = cart.GetCount(),
                    ItemCount = itemCount,
                    DeleteId = id
                };
            }
            catch
            {
                results = new ShoppingCartRemoveViewModel
                {
                    Message = "Error occurred or invalid input...",
                    CartTotal = -1,
                    CartCount = -1,
                    ItemCount = -1,
                    DeleteId = id
                };
            }
            return Json(results);
        }
        // 
        // AJAX: /ShoppingCart/RemoveFromCart/5 
        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            // Remove the item from the cart 
            var cart = ShoppingCart.GetCart(this.HttpContext);
            // TODO
            ShoppingCartRepository storeDB = new ShoppingCartRepository();
            string collectibleTitle = storeDB.Carts()
                .Single(item => item.RecordId == id).Title;

            // Remove from cart 
            int itemCount = cart.RemoveFromCart(id);
            // Display the confirmation message 
            var results = new ShoppingCartRemoveViewModel
            {
                Message = Server.HtmlEncode(collectibleTitle) +
                    " has been removed from your shopping cart.",
                CartTotal = cart.GetTotal(),
                CartCount = cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = id
            };
            return Json(results);
        }
        // 
        // GET: /ShoppingCart/CartSummary 
        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            ViewData["CartCount"] = cart.GetCount();
            return PartialView("CartSummary");
        }
    }
}