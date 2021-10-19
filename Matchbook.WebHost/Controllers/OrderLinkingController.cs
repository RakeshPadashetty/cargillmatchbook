using Matchbook.Db;
using Matchbook.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Matchbook.WebHost.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderLinkingController : ControllerBase
    {
        private readonly MatchbookDbContext dbContext;

        public OrderLinkingController(MatchbookDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        public ActionResult Post(List<int> input)
        {
            var testOrder = dbContext.Orders.Where(x => x.Id == 1).FirstOrDefault();
            try
            {
                //List<int> input = new List<int> { 1, 21 };
                if (input != null && input.Count > 0)
                {
                    List<OrderDTO> orderDtoList = new List<OrderDTO>();
                    List<Order> orders = new List<Order>();
                    foreach (var item in input)
                    {
                        orderDtoList.Add(dbContext.Orders.Where(x => x.Id == item).Select(y =>
                        new OrderDTO
                        {
                            linkId = y.LinkId,
                            ProductSymbol = y.ProductSymbol,
                            SubAccountId = y.SubAccountId
                        }).FirstOrDefault());
                        orders.Add(dbContext.Orders.Where(x => x.Id == item).FirstOrDefault());
                    }

                    if (orderDtoList.Select(x => x.linkId).Distinct().Count() > 1)
                    {
                        throw new Exception("Please pass the different orders since given orders are already linked to the same linkId");
                    }

                    if (orderDtoList.Select(x => x.ProductSymbol).Distinct().Count() == 1 && orderDtoList.Select(x => x.SubAccountId).Distinct().Count() == 1)
                    {
                        var orderLink = new OrderLink { LinkName = "Cargill" };
                        dbContext.Add(orderLink);

                        foreach (var order in orders)
                        {
                            order.Link = orderLink;
                        }
                        var result = dbContext.SaveChanges();
                        return Created("Saved", result);
                    }
                    else
                    {
                        throw new Exception("Orders cannot be linked because ProductSymbol and SubAccount are different for the given orders.");
                    }
                }
                else
                {
                    throw new Exception("Please pass the valid orders.");
                }
            }
            catch (Exception e)
            {
                return Ok(e);
            }
        }

        class OrderDTO
        {
            public int? linkId;
            public string ProductSymbol;
            public int SubAccountId;
        }
    }
}
