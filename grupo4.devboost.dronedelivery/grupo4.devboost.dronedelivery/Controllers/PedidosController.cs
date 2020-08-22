using Dapper;
using grupo4.devboost.dronedelivery.Data;
using grupo4.devboost.dronedelivery.Models;
using grupo4.devboost.dronedelivery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace grupo4.devboost.dronedelivery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly grupo4devboostdronedeliveryContext _context;
        private readonly IPedidoService _pedidoService;

        public PedidosController(grupo4devboostdronedeliveryContext context, IPedidoService pedidoService)
        {
            _context = context;
            _pedidoService = pedidoService;
        }

        // GET: api/Pedidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedido()
        {
            return await _context.Pedido.ToListAsync();
        }

        // GET: api/Pedidos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedido.FindAsync(id);

            if (pedido == null)
            {
                return NotFound();
            }

            return pedido;
        }

        [HttpGet("PedidosByDrone")]
        [Route("PedidosByDrone/{id?}")]
        public async Task<ActionResult<List<Pedido>>> PedidosByDrone(int id)
        {
            var pedidos = await _context.Pedido.ToListAsync();

            var pedidosByDrone = pedidos.Where(_ => _.DroneId == id).ToList();

            return Ok(pedidosByDrone);

        }

        // PUT: api/Pedidos/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, Pedido pedido)
        {
            if (id != pedido.Id)
            {
                return BadRequest();
            }

            _context.Entry(pedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Pedidos
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
        {
            pedido.DroneId = null;
            pedido.DataHoraInclusao = DateTime.Now;

            DroneDTO droneDTO = await _pedidoService.DroneAtendePedido(pedido);

            if (droneDTO != null)
            {
                pedido.DroneId = droneDTO.Drone.Id;
                pedido.Situacao = (int)EStatusPedido.DRONE_ASSOCIADO;

                var calculo = (droneDTO.Distancia / droneDTO.Drone.Velocidade);

                /*
                A cada pedido que o drone atende, vamos assumir como regra que ele sempre deve voltar para base,
                independente se ainda tem autonomia disponivel. Neste caso, estamos considerando no tempo de
                finalizacao + 1 para considerar o tempo de carga da bateria
                */
                pedido.DataHoraFinalizacao = DateTime.Now.AddHours((calculo +1));
            }
            else { 
                pedido.Situacao = (int)EStatusPedido.RECUSADO;
                pedido.DataHoraFinalizacao = DateTime.Now;
            }
            _context.Pedido.Add(pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedido", new { id = pedido.Id }, pedido);
        }

        // DELETE: api/Pedidos/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Pedido>> DeletePedido(int id)
        {
            var pedido = await _context.Pedido.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            _context.Pedido.Remove(pedido);
            await _context.SaveChangesAsync();

            return pedido;
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedido.Any(e => e.Id == id);
        }
    }
}
