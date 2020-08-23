using grupo4.devboost.dronedelivery.Data;
using grupo4.devboost.dronedelivery.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace grupo4.devboost.dronedelivery.Services
{
    public class DroneService : IDroneService
    {
        private readonly grupo4devboostdronedeliveryContext _context;
        const int tempoCargaDronePadrao = 1;


        public DroneService(grupo4devboostdronedeliveryContext context)
        {
            _context = context;
        }

        public async Task<List<Drone>> GetAll()
        {
           var drones =  _context.Drone.ToList();


            //verifica que 
            //var dataHoraFinalizacaoAtividadeDrone = drones.pedidos.Max(m => m.DataHoraFinalizacao).AddHours(tempoCargaDronePadrao);
           

            //atribui os pedidos correntes aos drones
            drones.ForEach(d => d.pedidos = _context.Pedido.Where(p =>  p.DroneId == d.Id && p.DataHoraFinalizacao > DateTime.Now).ToList());

            //garante que os drones ociosos estarao em primeiro na fila
            return drones.OrderBy(o => o.pedidos.Count()).ToList();
        }

    }
}
