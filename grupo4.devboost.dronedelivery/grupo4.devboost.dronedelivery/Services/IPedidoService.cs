using grupo4.devboost.dronedelivery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace grupo4.devboost.dronedelivery.Services
{
    public interface IPedidoService
    {
        Task<DroneDTO> AtribuirPedidoDrone(Pedido pedido);
        public bool DroneAtendeMaisUmPedido(Drone droneDTO, Pedido novoPedido);
    }
}
