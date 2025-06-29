﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BestStore.Data;
using BestStore.Models;

namespace BestStore.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Products/Produit
        public async Task<IActionResult> Produit()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();

            var productDetailsList = new List<ProductDetailsViewModel>();

            foreach (var product in products)
            {
                var comments = await _context.Comments
                    .Where(c => c.ProductId == product.Id)
                    .ToListAsync();

                var likeCount = await _context.Likes
                    .CountAsync(l => l.ProductId == product.Id);

                var productDetails = new ProductDetailsViewModel
                {
                    Product = product,
                    Comments = comments,
                    LikeCount = likeCount,
                    UserHasLiked = false // Simplification, no user auth
                };

                productDetailsList.Add(productDetails);
            }

            return View(productDetailsList);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var comments = await _context.Comments
                .Where(c => c.ProductId == id)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            var likeCount = await _context.Likes
                .CountAsync(l => l.ProductId == id);

            // For simplicity, user like status is false (no user auth implemented)
            var userHasLiked = false;

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Comments = comments,
                LikeCount = likeCount,
                UserHasLiked = userHasLiked
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int productId, string emoji, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            string userName;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                userName = User.Identity.Name ?? "inconnu";
            }
            else
            {
                userName = "inconnu";
            }

            var comment = new Comment
            {
                ProductId = productId,
                UserName = userName,
                Emoji = emoji,
                Content = content,
                CreatedDate = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int productId)
        {
            // For simplicity, using a fixed user name
            string userName = "Anonymous";

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.ProductId == productId && l.UserName == userName);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                var like = new Like
                {
                    ProductId = productId,
                    UserName = userName,
                    CreatedDate = DateTime.Now
                };
                _context.Likes.Add(like);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = productId });
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,CategoryId,ImageFileName,CreatedDate,ProductId")] Product product, Microsoft.AspNetCore.Http.IFormFile ImageFileName)
        {
            if (ModelState.IsValid)
            {
                if (ImageFileName != null && ImageFileName.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFileName.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFileName.CopyToAsync(stream);
                    }

                    product.ImageFileName = uniqueFileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,CategoryId,ImageFileName,CreatedDate,ProductId")] Product product, Microsoft.AspNetCore.Http.IFormFile ImageFileName)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (ImageFileName != null && ImageFileName.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploadsFolder);
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFileName.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFileName.CopyToAsync(stream);
                    }

                    product.ImageFileName = uniqueFileName;
                }

                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
