let solicitudes = [];

document.addEventListener("DOMContentLoaded", async () => {
  if (!requireRole("Admin")) return;
  document.getElementById("userName").textContent = sessionStorage.getItem("nombre") || "";
  document.getElementById("refreshButton").addEventListener("click", cargarSolicitudes);
  document.getElementById("closeRequest").addEventListener("click", cerrarDetalle);
  await cargarSolicitudes();
});

async function cargarSolicitudes() {
  const grid = document.getElementById("requestsGrid");
  try {
    solicitudes = await apiGet("/solicitudes/pendientes");
    grid.innerHTML = solicitudes.length ? solicitudes.map(s => `<article class="request-card" onclick="verSolicitud(${s.id})">
      <div class="request-card-top"><strong>${escapeHtml(s.nombre)} ${escapeHtml(s.apellido)}</strong><span>Pendiente</span></div>
      <div>DNI: ${escapeHtml(s.dni)}</div><div>${escapeHtml(s.direccion)}</div>
      <small>${formatearFecha(s.fecha)} · ${String(s.hora).slice(0,5)}</small>
      <button class="mini" onclick="event.stopPropagation();verSolicitud(${s.id})">Ver detalle</button>
    </article>`).join("") : "<div class='panel'>No hay solicitudes pendientes.</div>";
  } catch (e) { const el = document.getElementById("requestError"); el.textContent = e.message; el.classList.remove("hidden"); }
}
async function verSolicitud(id) {
  try {
    const s = await apiGet(`/solicitudes/${id}`);
    document.getElementById("requestDetail").innerHTML = `<div class="request-detail">
      <div><small>Nombre</small><strong>${escapeHtml(s.nombre)}</strong></div><div><small>Apellido</small><strong>${escapeHtml(s.apellido)}</strong></div>
      <div><small>DNI</small><strong>${escapeHtml(s.dni)}</strong></div><div><small>Dirección</small><strong>${escapeHtml(s.direccion)}</strong></div>
      <div><small>Fecha</small><strong>${formatearFecha(s.fecha)}</strong></div><div><small>Hora</small><strong>${String(s.hora).slice(0,5)}</strong></div>
    </div><div class="request-actions"><button class="danger" onclick="rechazarSolicitud(${s.id})">Rechazar</button><button onclick="aceptarSolicitud(${s.id})">Aceptar</button></div>`;
    document.getElementById("requestModal").classList.remove("hidden");
  } catch (e) { alert(e.message); }
}
function cerrarDetalle(){document.getElementById("requestModal").classList.add("hidden");}
async function rechazarSolicitud(id){
  try { await apiPut(`/solicitudes/${id}/estado`, {estado:true}); location.href="admin.html"; }
  catch(e){alert(e.message);}
}
function aceptarSolicitud(id){ location.href=`alta-usuario.html?solicitudId=${encodeURIComponent(id)}`; }
function formatearFecha(v){return new Date(v).toLocaleDateString("es-AR");}
function escapeHtml(v){return String(v).replace(/[&<>"']/g,c=>({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#039;"}[c]));}
