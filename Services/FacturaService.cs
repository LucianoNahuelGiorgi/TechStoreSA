using System.Text;
using TechStoreSA.Models;
using System.Globalization;

namespace TechStoreSA.Services
{
    public class FacturaService
    {
        public string GenerarFacturaHtml(Venta venta)
        {
            var sb = new StringBuilder();
            var culture = new CultureInfo("es-AR");

            // Estilos CSS para que se vea como una factura real
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; color: #333; }");
            sb.AppendLine(".header { text-align: center; margin-bottom: 40px; border-bottom: 2px solid #009688; padding-bottom: 10px; }");
            sb.AppendLine(".header h1 { margin: 0; color: #009688; }");
            sb.AppendLine(".info-table { width: 100%; margin-bottom: 20px; }");
            sb.AppendLine(".info-table td { padding: 5px; vertical-align: top; }");
            sb.AppendLine(".items-table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine(".items-table th { background-color: #009688; color: white; padding: 10px; text-align: left; }");
            sb.AppendLine(".items-table td { border: 1px solid #ddd; padding: 8px; }");
            sb.AppendLine(".totals { float: right; width: 300px; margin-top: 20px; }");
            sb.AppendLine(".totals table { width: 100%; border-collapse: collapse; }");
            sb.AppendLine(".totals td { padding: 5px; text-align: right; }");
            sb.AppendLine(".total-final { font-size: 1.2em; font-weight: bold; color: #009688; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Encabezado
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>TechStore S.A.</h1>");
            sb.AppendLine("<p>Factura de Venta</p>");
            sb.AppendLine($"<p>Nro. de Operación: {venta.Id.ToString("D8")}</p>"); // Formato 00000123
            sb.AppendLine("</div>");

            // Datos del Cliente y la Venta
            sb.AppendLine("<table class='info-table'>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td width='50%'>");
            sb.AppendLine("<strong>Datos del Cliente:</strong><br>");
            sb.AppendLine($"{venta.Cliente.NombreCompleto}<br>");
            sb.AppendLine($"DNI/CUIT: {venta.Cliente.Documento}<br>");
            sb.AppendLine($"Tipo: {venta.Cliente.Tipo}");
            sb.AppendLine("</td>");
            sb.AppendLine("<td width='50%'>");
            sb.AppendLine("<strong>Detalles de la Operación:</strong><br>");
            sb.AppendLine($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}<br>");
            sb.AppendLine($"Sucursal: {venta.Sucursal?.Nombre ?? "N/A"}<br>");
            sb.AppendLine($"Vendedor: {venta.Vendedor?.NombreCompleto ?? "N/A"}<br>");
            sb.AppendLine($"Forma de Pago: {venta.MetodoPago}");
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");

            // Tabla de Productos
            sb.AppendLine("<table class='items-table'>");
            sb.AppendLine("<thead><tr><th>Producto</th><th>Cant.</th><th>Precio Unit.</th><th>Subtotal</th></tr></thead>");
            sb.AppendLine("<tbody>");

            foreach (var item in venta.Detalles)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{item.Producto.Nombre}</td>");
                sb.AppendLine($"<td>{item.Cantidad}</td>");
                sb.AppendLine($"<td>{item.PrecioUnitario.ToString("C2", culture)}</td>");
                sb.AppendLine($"<td>{item.Importe.ToString("C2", culture)}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Totales
            sb.AppendLine("<div class='totals'>");
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr><td>Subtotal:</td><td>{venta.SubTotal.ToString("C2", culture)}</td></tr>");
            
            if (venta.DescuentoAplicado > 0)
            {
                sb.AppendLine($"<tr><td>Descuento:</td><td>-{venta.DescuentoAplicado.ToString("C2", culture)}</td></tr>");
            }

            sb.AppendLine($"<tr class='total-final'><td>TOTAL:</td><td>{venta.Total.ToString("C2", culture)}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");

            return sb.ToString();
        }
    }
}