const tankImages = ["assets/tank-water.svg", "assets/tank-metal.svg", "assets/tank-plastic.svg", "assets/tank-elevated.svg"];

document.addEventListener("DOMContentLoaded", async () => {
  if (!requireRole("Admin")) return;
  document.getElementById("userName").textContent = sessionStorage.getItem("nombre") || "";
  document.getElementById("refreshButton").addEventListener("click", cargarTanquesAdmin);
  await cargarTanquesAdmin();
});

async function cargarTanquesAdmin() {
  const grid = document.getElementById("tankGridAdmin");
  const error = document.getElementById("tankError");
  try {
    const tanques = await apiGet("/tanques");
    if (!tanques.length) { grid.innerHTML = "<div class='panel'>No hay tanques registrados.</div>"; return; }
    grid.innerHTML = tanques.map((t, i) => {
      const pct = Math.max(0, Math.min(100, Number(t.porcentajeLlenado)));
      return `<article class="tank-admin-card" onclick="location.href='tanque-info.html?id=${t.id}'">
        <img src="${tankImages[i % tankImages.length]}" alt="Tanque ${t.id}">
        <div class="tank-admin-id">Tanque #${t.id}</div>
        <div class="tank-admin-amount">${Number(t.capacidadActualCM3).toFixed(2)} cm³</div>
        <div class="tank-admin-percent">${pct.toFixed(2)}%</div>
        <div class="level-bar"><div class="level-fill" style="width:${pct}%"></div></div>
      </article>`;
    }).join("");
    error.classList.add("hidden");
  } catch (e) { error.textContent = e.message; error.classList.remove("hidden"); }
}
