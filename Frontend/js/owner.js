document.addEventListener("DOMContentLoaded", async () => {
  if (!requireRole("Dueño")) return;
  document.getElementById("userName").textContent = sessionStorage.getItem("nombre") || "";
  document.getElementById("refreshButton").addEventListener("click", cargarTanques);
  document.getElementById("tankGrid").addEventListener("click", manejarAccionTanque);
  await cargarTanques();
  setInterval(cargarTanques, 10000);
});

async function cargarTanques() {
  const grid = document.getElementById("tankGrid");
  const error = document.getElementById("ownerError");
  try {
    const tanques = await apiGet(`/tanques?usuarioId=${encodeURIComponent(sessionStorage.getItem("usuarioId"))}`);
    renderTanques(tanques);
    error.classList.add("hidden");
  } catch (e) {
    error.textContent = e.message;
    error.classList.remove("hidden");
  }
}

function renderTanques(tanques) {
  const grid = document.getElementById("tankGrid");
  if (!tanques.length) {
    grid.innerHTML = "<div class='panel'>No tienes tanques asignados.</div>";
    return;
  }
  grid.innerHTML = tanques.map(t => `
    <article class="tank-card">
      <h2>Tanque #${Number(t.id)}</h2>
      <div class="tank-meta">${escapeHtml(t.tipoContenido)} · ${escapeHtml(t.ubicacion)}</div>
      <div class="level-bar"><div class="level-fill" style="width:${Math.max(0, Math.min(100, Number(t.porcentajeLlenado) || 0))}%"></div></div>
      <div class="tank-value">${Number(t.capacidadActualCM3).toFixed(2)} cm³</div>
      <div class="tank-percent">${Number(t.porcentajeLlenado).toFixed(2)}% lleno</div>
      <div class="tank-details">
        <span>Capacidad: ${Number(t.capacidadTotalCM3).toFixed(2)} cm³</span>
        <span>Diámetro: ${Number(t.diametro).toFixed(2)} cm</span>
        <span>Altura: ${Number(t.alturaCM).toFixed(2)} cm</span>
      </div>
      <div class="tank-actions">
        <button type="button" data-action="auto-fill" data-tank-id="${Number(t.id)}">Auto Llenado</button>
      </div>
    </article>`).join("");
}

async function manejarAccionTanque(event) {
  const button = event.target.closest("button[data-action='auto-fill']");
  if (!button) return;

  const tankId = Number(button.dataset.tankId);
  const notice = document.getElementById("ownerNotice");
  const error = document.getElementById("ownerError");
  button.disabled = true;
  button.textContent = "Solicitando llenado…";
  notice.classList.add("hidden");
  error.classList.add("hidden");

  try {
    const result = await apiPost(`/tanques/${tankId}/auto-llenado`, {});
    notice.textContent = result.message || `Comando enviado al Arduino para llenar el tanque #${tankId} hasta el 80 %.`;
    notice.classList.remove("hidden");
    await cargarTanques();
  } catch (e) {
    error.textContent = e.message;
    error.classList.remove("hidden");
  } finally {
    button.disabled = false;
    button.textContent = "Auto Llenado";
  }
}

function escapeHtml(v) {
  return String(v ?? "").replace(/[&<>"']/g, c => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;"
  }[c]));
}
