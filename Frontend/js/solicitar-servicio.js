document.addEventListener("DOMContentLoaded", () => {
  const now = new Date();
  const pad = n => String(n).padStart(2, "0");
  document.getElementById("fecha").value = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
  document.getElementById("hora").value = `${pad(now.getHours())}:${pad(now.getMinutes())}`;

  const form = document.getElementById("serviceForm");
  const message = document.getElementById("serviceMessage");
  const button = document.getElementById("sendButton");


  form.addEventListener("submit", async e => {
    e.preventDefault();
    message.className = "alert hidden";
    button.disabled = true;
    button.textContent = "Enviando...";
    try {
      await apiPost("/solicitudes", {
        nombre: document.getElementById("nombre").value.trim(),
        apellido: document.getElementById("apellido").value.trim(),
        dni: document.getElementById("dni").value.trim(),
        direccion: document.getElementById("direccion").value.trim(),
        fecha: document.getElementById("fecha").value,
        hora: document.getElementById("hora").value + ":00"
      });
      message.textContent = "Solicitud enviada correctamente. El administrador revisará tus datos.";
      message.className = "alert success";
      form.reset();
    } catch (err) {
      message.textContent = err.message;
      message.className = "alert error";
    } finally {
      button.disabled = false;
      button.textContent = "Enviar Solicitud";
    }
  });
});
