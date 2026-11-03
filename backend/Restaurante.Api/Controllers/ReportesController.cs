using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Domain.Enums;
using Restaurante.Api.Infrastructure.Data;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Administrador")]
public class ReportesController : ControllerBase
{
    private readonly RestauranteDbContext _db;

    public ReportesController(RestauranteDbContext db)
    {
        _db = db;
    }

    [HttpGet("ventas-diarias")]
    public async Task<IActionResult> GetVentasDiarias([FromQuery] DateTime? fecha)
    {
        var diaConsulta = (fecha ?? DateTime.UtcNow).Date;

        // Buscar facturas del día seleccionado
        var facturas = await _db.Facturas
            .Where(f => f.Fecha.Date == diaConsulta)
            .ToListAsync();

        if (!facturas.Any())
        {
            return Ok(new
            {
                fecha = diaConsulta.ToString("yyyy-MM-dd"),
                totalVentas = 0,
                cantidadPedidos = 0,
                ticketPromedio = 0,
                ventasPorMetodo = new { efectivo = 0, tarjeta = 0, transferencia = 0 }
            });
        }

        var totalVentas = facturas.Sum(f => f.Total);
        var cantidadPedidos = facturas.Count;
        
        var ventasPorMetodo = new
        {
            efectivo = facturas.Where(f => f.MetodoPago == MetodoPago.Efectivo).Sum(f => f.Total),
            tarjeta = facturas.Where(f => f.MetodoPago == MetodoPago.Tarjeta).Sum(f => f.Total),
            transferencia = facturas.Where(f => f.MetodoPago == MetodoPago.Transferencia).Sum(f => f.Total)
        };

        return Ok(new
        {
            fecha = diaConsulta.ToString("yyyy-MM-dd"),
            totalVentas = totalVentas,
            cantidadPedidos = cantidadPedidos,
            ticketPromedio = totalVentas / cantidadPedidos,
            ventasPorMetodo = ventasPorMetodo
        });
    }

        [HttpGet("ventas-mensuales")]
    public async Task<IActionResult> GetVentasMensuales([FromQuery] int? anio, [FromQuery] int? mes)
    {
        var anioConsulta = anio ?? DateTime.UtcNow.Year;
        var mesConsulta = mes ?? DateTime.UtcNow.Month;

        var inicioMes = new DateTime(anioConsulta, mesConsulta, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        var inicioMesAnterior = inicioMes.AddMonths(-1);
        var finMesAnterior = inicioMes.AddDays(-1);

        // Ventas del mes consultado
        var facturasMes = await _db.Facturas
            .Where(f => f.Fecha >= inicioMes && f.Fecha <= finMes)
            .ToListAsync();

        // Ventas del mes anterior
        var facturasMesAnterior = await _db.Facturas
            .Where(f => f.Fecha >= inicioMesAnterior && f.Fecha <= finMesAnterior)
            .ToListAsync();

        var totalMes = facturasMes.Sum(f => f.Total);
        var totalMesAnterior = facturasMesAnterior.Sum(f => f.Total);
        
        // Variación porcentual
        double variacion = 0;
        if (totalMesAnterior > 0)
        {
            variacion = (double)((totalMes - totalMesAnterior) / totalMesAnterior) * 100;
        }

        return Ok(new
        {
            periodo = $"{mesConsulta}/{anioConsulta}",
            totalMes = totalMes,
            totalMesAnterior = totalMesAnterior,
            variacionPorcentual = Math.Round(variacion, 2),
            cantidadPedidos = facturasMes.Count,
            ticketPromedio = facturasMes.Count > 0 ? totalMes / facturasMes.Count : 0
        });
    }
}