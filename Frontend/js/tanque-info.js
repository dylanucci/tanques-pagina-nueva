document.addEventListener("DOMContentLoaded", async () => {
  if (!requireRole("Admin")) return;
  document.getElementById("userName").textContent = sessionStorage.getItem("nombre") || "";
  const id = new URLSearchParams(location.search).get("id");
  if (!id) return mostrarError("No se indicó el ID del tanque.");
  try {
    const t = await apiGet(`/tanques/${encodeURIComponent(id)}`);
    const pct = Math.max(0, Math.min(100, Number(t.porcentajeLlenado)));
    document.getElementById("tankInfo").innerHTML = `<section class="panel tank-detail">
      <div class="tank-detail-head"><div><span class="detail-label">Tanque</span><h1>#${t.id}</h1><p>${escapeHtml(t.tipoContenido)}</p></div><img src="assets/tank-water.svg" alt="Tanque"></div>
      <div class="level-bar"><div class="level-fill" style="width:${pct}%"></div></div><div class="detail-percent">${pct.toFixed(2)}% lleno</div>
      <div class="info-grid">
        <div><small>ID</small><strong>${t.id}</strong></div>
        <div><small>Capacidad actual</small><strong>${Number(t.capacidadActualCM3).toFixed(2)} cm³</strong></div>
        <div><small>Capacidad total</small><strong>${Number(t.capacidadTotalCM3).toFixed(2)} cm³</strong></div>
        <div><small>Diámetro</small><strong>${Number(t.diametro).toFixed(2)} cm</strong></div>
        <div><small>Altura</small><strong>${Number(t.alturaCM).toFixed(2)} cm</strong></div>
        <div><small>Ubicación</small><strong>${escapeHtml(t.ubicacion)}</strong></div>
      </div>
      <div class="panel-sub"><h2>Usuarios asignados</h2>${t.usuarios.length ? `<ul>${t.usuarios.map(u => `<li>${escapeHtml(u.nombre)} — ${escapeHtml(u.usuario)}</li>`).join("")}</ul>` : "<p>Sin usuarios asignados.</p>"}</div>
    </section>`;
  } catch (e) { mostrarError(e.message); }
});
function mostrarError(msg){const el=document.getElementById("tankError");el.textContent=msg;el.classList.remove("hidden");}
function escapeHtml(v){return String(v).replace(/[&<>"']/g,c=>({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#039;"}[c]));}
