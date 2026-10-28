let solicitud = null;

document.addEventListener("DOMContentLoaded", async () => {
  if (!requireRole("Admin")) return;
  document.getElementById("userName").textContent = sessionStorage.getItem("nombre") || "";
  const id = new URLSearchParams(location.search).get("solicitudId");
  if (!id) return mostrar("No se indicó la solicitud.", true);
  try {
    solicitud = await apiGet(`/solicitudes/${encodeURIComponent(id)}`);
    if (solicitud.estado) return mostrar("La solicitud ya fue atendida.", true);
    document.getElementById("requestFields").innerHTML = `<div><small>Nombre</small><strong>${escapeHtml(solicitud.nombre)}</strong></div><div><small>Apellido</small><strong>${escapeHtml(solicitud.apellido)}</strong></div><div><small>DNI</small><strong>${escapeHtml(solicitud.dni)}</strong></div><div><small>Dirección</small><strong>${escapeHtml(solicitud.direccion)}</strong></div><div><small>Fecha</small><strong>${new Date(solicitud.fecha).toLocaleDateString("es-AR")}</strong></div><div><small>Hora</small><strong>${String(solicitud.hora).slice(0,5)}</strong></div>`;
  } catch(e){ mostrar(e.message, true); }

  document.getElementById("altaForm").addEventListener("submit", darAlta);
});

async function darAlta(e){
  e.preventDefault();
  if (!solicitud) return;
  const button=document.getElementById("altaButton"); button.disabled=true; button.textContent="Creando...";
  try {
    const tanqueId = Number(document.getElementById("tanqueId").value);
    await apiGet(`/tanques/${tanqueId}`);
    const usuario = await apiPost("/usuarios", { nombre: `${solicitud.nombre} ${solicitud.apellido}`.trim(), usuario: document.getElementById("usuario").value.trim(), contrasena: document.getElementById("contrasena").value, rolId: 1 });
    await apiPost(`/tanques/${tanqueId}/usuarios/${usuario.id}`, {});
    await apiPut(`/solicitudes/${solicitud.id}/estado`, {estado:true});
    mostrar("Usuario dado de alta y tanque asignado correctamente. Volviendo al panel...", false);
    setTimeout(() => location.href="admin.html", 1200);
  } catch(e) {
    mostrar(e.message, true);
  } finally { button.disabled=false; button.textContent="Dar Alta"; }
}
function mostrar(msg,error){const el=document.getElementById("altaMessage");el.textContent=msg;el.className=`alert ${error?'error':'success'}`;}
function escapeHtml(v){return String(v).replace(/[&<>"']/g,c=>({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#039;"}[c]));}
