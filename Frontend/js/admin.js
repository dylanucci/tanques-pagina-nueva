let usuarios = [], tanques = [];

document.addEventListener("DOMContentLoaded", async () => {
  if (!requireRole("Admin")) return;
  document.getElementById("userName").textContent = sessionStorage.getItem("nombre") || "";
  document.getElementById("refreshButton").addEventListener("click", cargarDashboard);
  document.getElementById("requestsButton").addEventListener("click", () => location.href = "solicitudes.html");
  document.getElementById("newUserButton").addEventListener("click", formUsuario);
  document.getElementById("newTankButton").addEventListener("click", formTanque);
  document.getElementById("closeModal").addEventListener("click", cerrarModal);
  document.getElementById("stats").addEventListener("click", e => {
    if (e.target.closest("[data-tanks-link]")) location.href = "tanques-admin.html";
  });
  await cargarDashboard();
  setInterval(cargarDashboard, 10000);
});

async function cargarDashboard() {
  try {
    [usuarios, tanques] = await Promise.all([apiGet("/usuarios"), apiGet("/tanques")]);
    renderStats(); renderUsuarios(); renderTanques();
  } catch (e) { mostrarError(e.message); }
}
function renderStats() {
  document.getElementById("stats").innerHTML = [
    ["Usuarios", usuarios.length, ""],
    ["Dueños", usuarios.filter(x => x.rol === "Dueño").length, ""],
    ["Tanques", tanques.length, "data-tanks-link"],
    ["Nivel promedio", tanques.length ? Math.round(tanques.reduce((a, x) => a + Number(x.porcentajeLlenado), 0) / tanques.length) + "%" : "0%", ""]
  ].map(x => `<div class="stat ${x[2] ? "stat-clickable" : ""}" ${x[2]}>${x[2] ? "<span class='stat-link'>Ver tanques</span>" : ""}<small>${x[0]}</small><strong>${x[1]}</strong></div>`).join("");
}
function renderUsuarios() {
  document.getElementById("usersBody").innerHTML = usuarios.map(u => `<tr>
    <td>${u.id}</td><td>${escapeHtml(u.nombre)}</td><td>${escapeHtml(u.usuario)}</td><td>${escapeHtml(u.rol)}</td>
    <td class="actions-cell"><button class="danger mini" onclick="eliminarUsuario(${u.id})">Eliminar</button></td>
  </tr>`).join("");
}
function renderTanques() {
  document.getElementById("tanksBody").innerHTML = tanques.map(t => `<tr>
    <td>${t.id}</td><td>${escapeHtml(t.tipoContenido)}</td>
    <td>${t.capacidadActualCM3} cm³</td><td>${t.capacidadTotalCM3} cm³</td><td>${t.porcentajeLlenado}%</td>
    <td>${Number(t.alturaCM).toFixed(2)} cm</td><td>${escapeHtml(t.ubicacion)}</td><td>${t.usuarios.map(u => escapeHtml(u.nombre)).join(", ") || "Sin asignar"}</td>
    <td class="actions-cell"><button class="danger mini" onclick="eliminarTanque(${t.id})">Eliminar</button><button class="mini" onclick="asignarTanque(${t.id})">Asignar</button></td>
  </tr>`).join("");
}
function abrirModal(title, html) { document.getElementById("modalTitle").textContent = title; document.getElementById("modalForm").innerHTML = html; document.getElementById("modal").classList.remove("hidden"); }
function cerrarModal() { document.getElementById("modal").classList.add("hidden"); }
function formUsuario() {
  abrirModal("Nuevo usuario", `
    <label>Nombre<input id="fNombre" required></label>
    <label>Usuario<input id="fUsuario" required></label>
    <label>Contraseña<input id="fPassword" type="password" required minlength="6"></label>
    <label>Rol<select id="fRol"><option value="1">Dueño</option><option value="2">Admin</option></select></label>
    <div class="actions"><button type="button" onclick="cerrarModal()" class="secondary">Cancelar</button><button type="button" onclick="crearUsuario()">Crear</button></div>`);
}
async function crearUsuario() {
  try { await apiPost("/usuarios", { nombre: f("fNombre"), usuario: f("fUsuario"), contrasena: f("fPassword"), rolId: Number(f("fRol")) }); cerrarModal(); await cargarDashboard(); }
  catch (e) { alert(e.message); }
}
function formTanque() {
  abrirModal("Nuevo tanque", `
    <label>Tipo de contenido<input id="fTipo" required></label>
    <label>Diámetro (cm)<input id="fDiametro" type="number" min="0.01" step="0.01" required></label>
    <label>Altura (cm)<input id="fAltura" type="number" min="0.01" step="0.01" required></label>
    <label>Ubicación<input id="fUbicacion" required></label>
    <label>Dueño opcional<select id="fUsuario"><option value="">Sin asignar</option>${usuarios.filter(x => x.rol === "Dueño").map(u => `<option value="${u.id}">${escapeHtml(u.nombre)}</option>`).join("")}</select></label>
    <div class="actions"><button type="button" onclick="cerrarModal()" class="secondary">Cancelar</button><button type="button" onclick="crearTanque()">Crear</button></div>`);
}
async function crearTanque() {
  try {
    const usuario = f("fUsuario");
    await apiPost("/tanques", { tipoContenido: f("fTipo"), capacidadActualCM3: 0, capacidadTotalCM3: 0, diametro: Number(f("fDiametro")), alturaCM: Number(f("fAltura")), ubicacion: f("fUbicacion"), usuarioId: usuario ? Number(usuario) : null });
    cerrarModal(); await cargarDashboard();
  } catch (e) { alert(e.message); }
}
async function eliminarUsuario(id) { if (!confirm("¿Eliminar usuario?")) return; try { await apiDelete(`/usuarios/${id}`); await cargarDashboard(); } catch (e) { mostrarError(e.message); } }
async function eliminarTanque(id) { if (!confirm("¿Eliminar tanque?")) return; try { await apiDelete(`/tanques/${id}`); await cargarDashboard(); } catch (e) { mostrarError(e.message); } }
async function asignarTanque(tanqueId) {
  const duenos = usuarios.filter(x => x.rol === "Dueño");
  const nombre = prompt("Escribe el ID del dueño:\n" + duenos.map(x => `${x.id} - ${x.nombre}`).join("\n"));
  if (!nombre) return;
  try { await apiPost(`/tanques/${tanqueId}/usuarios/${Number(nombre)}`, {}); await cargarDashboard(); } catch (e) { mostrarError(e.message); }
}
function f(id) { return document.getElementById(id).value.trim(); }
function mostrarError(msg) { const el = document.getElementById("adminError"); el.textContent = msg; el.classList.remove("hidden"); }
function escapeHtml(v) { return String(v).replace(/[&<>"']/g, c => ({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#039;"}[c])); }
