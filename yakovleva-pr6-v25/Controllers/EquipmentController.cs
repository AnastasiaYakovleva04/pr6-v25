using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using yakovleva_pr6_v25.Models;

namespace yakovleva_pr6_v25.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EquipmentController: ControllerBase
    {
        private readonly WebApiDatabaseContext _context;

        public EquipmentController(WebApiDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetAll([FromQuery] decimal? minPrice, [FromQuery] bool? operational)
        {
            IQueryable<Equipment> query = _context.Equipment;
            if (operational != null)
                query = query.Where(e => e.IsOperational == operational);
            if (minPrice != null)
                query = query.Where(e => e.Price >= minPrice);
            var list = await query.ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Equipment>> GetById(int id)
        {
            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
                return NotFound();
            return Ok(equipment);
        }
        [HttpPost]
        public async Task<ActionResult<Equipment>> CreateEqiupment([FromBody] Equipment equipment)
        {
            if (!ModelState.IsValid)
                return BadRequest("Некорректно заполнены поля");
            
            if (equipment.Price <= 0)
                return BadRequest("Цена не может быть меньше 0");
            
            if (string.IsNullOrWhiteSpace(equipment.Name) || string.IsNullOrWhiteSpace(equipment.InventoryNumber))
                return BadRequest("Имя и инвентарный номер не могут быть пустыми");

            if (equipment.PurchaseDate > DateOnly.FromDateTime(DateTime.Now) || equipment.PurchaseDate.Year < 1990)
                return BadRequest("Дата приобретения не может быть в будущем или раньше 1990 года");

            var existingItem = await _context.Equipment.FirstOrDefaultAsync(e => e.InventoryNumber == equipment.InventoryNumber);
            if (existingItem != null)
                return BadRequest("Оборудование с таким инвентарным номером уже существует");

            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();
            return Created($"/api/v1/Equipment/{equipment.Id}", equipment);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEquipment([FromRoute] int id, [FromBody] Equipment equipment)
        {
            if (id != equipment.Id)
                return BadRequest("Id в адресе не совпадает с Id в теле запроса");
            
            if (equipment.Price <= 0)
                return BadRequest("Цена не может быть меньше 0");
            
            if (string.IsNullOrWhiteSpace(equipment.Name) || string.IsNullOrWhiteSpace(equipment.InventoryNumber))
                return BadRequest("Имя и инвентарный номер не могут быть пустыми");

            if (equipment.PurchaseDate > DateOnly.FromDateTime(DateTime.Now))
                return BadRequest("Дата приобретения не может быть в будущем.");

            var existingInvNum = await _context.Equipment.FirstOrDefaultAsync(e => e.InventoryNumber == equipment.InventoryNumber);
            if (existingInvNum != null)
                return BadRequest("Оборудование с таким инвентарным номером уже существует");


            var existingItem = await _context.Equipment.FindAsync(id);
            if (existingItem == null)
                return NotFound(); 

            existingItem.Name = equipment.Name;
            existingItem.InventoryNumber = equipment.InventoryNumber;
            existingItem.Price = equipment.Price;
            existingItem.PurchaseDate = equipment.PurchaseDate;
            existingItem.IsOperational = equipment.IsOperational;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> PatchEquipment([FromRoute] int id, [FromBody] Dictionary<string, object> updates)
        {
            var existingItem = await _context.Equipment.FindAsync(id);
            if (existingItem == null)
                return NotFound();

            if (updates.ContainsKey("name"))
                existingItem.Name = updates["name"].ToString();

            if (updates.ContainsKey("inventoryNumber"))
                existingItem.InventoryNumber = updates["inventoryNumber"].ToString();

            if (updates.ContainsKey("price"))
                existingItem.Price = Convert.ToDecimal(updates["price"].ToString());

            if (updates.ContainsKey("purchaseDate"))
                existingItem.PurchaseDate = DateOnly.Parse(updates["purchaseDate"].ToString());

            if (updates.ContainsKey("isOperational"))
                existingItem.IsOperational = bool.Parse(updates["isOperational"].ToString());

            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            var existingItem = await _context.Equipment.FindAsync(id);
            if (existingItem == null) 
                return NotFound();

             _context.Equipment.Remove(existingItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
