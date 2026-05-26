using BloggerAPI.Data;
using BloggerAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BloggerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public CategoriesController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToListAsync();
        }

        //POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> PostCategory(CategoryDto categoryDto)
        {
            var category = new Models.Category
            {
                Name = categoryDto.Name
            };
            _context.Categories.Add(category);
            await _context.UpdateDatabase();

            await _context.SaveChangesAsync();

            categoryDto.Id = category.Id;
            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, categoryDto);
        }
    }
}
