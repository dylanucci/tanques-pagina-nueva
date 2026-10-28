document.addEventListener("DOMContentLoaded", () => {
  if (sessionStorage.getItem("usuarioId")) {
    location.href = sessionStorage.getItem("rol") === "Admin" ? "admin.html" : "owner.html";
    return;
  }

  const form = document.getElementById("loginForm");
  const error = document.getElementById("loginError");
  const button = document.getElementById("loginButton");
  document.getElementById("serviceButton").addEventListener("click", () => location.href = "solicitar-servicio.html");

  form.addEventListener("submit", async e => {
    e.preventDefault();
    error.classList.add("hidden");
    button.disabled = true;
    button.textContent = "Ingresando...";
    try {
      const result = await apiPost("/auth/login", {
        usuario: document.getElementById("usuario").value.trim(),
        contrasena: document.getElementById("contrasena").value
      });
      saveSession(result.data);
      location.href = result.data.rol === "Admin" ? "admin.html" : "owner.html";
    } catch (err) {
      error.textContent = err.message;
      error.classList.remove("hidden");
    } finally {
      button.disabled = false;
      button.textContent = "Ingresar";
    }
  });
});
