function saveSession(data){
  sessionStorage.setItem("usuarioId", data.usuarioId);
  sessionStorage.setItem("nombre", data.nombre);
  sessionStorage.setItem("usuario", data.usuario);
  sessionStorage.setItem("rol", data.rol);
}
function logout(){ sessionStorage.clear(); location.href = "login.html"; }
function requireRole(role){
  const currentRole = sessionStorage.getItem("rol");
  const userId = sessionStorage.getItem("usuarioId");
  if (!userId || currentRole !== role) { location.href = "login.html"; return false; }
  return true;
}
document.addEventListener("DOMContentLoaded",()=>{ const b=document.getElementById("logoutButton"); if(b) b.addEventListener("click",logout); });
