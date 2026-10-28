const API_BASE = "http://localhost:3000/api";

async function apiRequest(path, options = {}) {
  const headers = new Headers(options.headers || {});
  if (options.body !== undefined) headers.set("Content-Type", "application/json");
  const response = await fetch(`${API_BASE}${path}`, { ...options, headers });
  if (response.status === 204) return null;
  const data = await response.json().catch(() => ({}));
  if (!response.ok) throw new Error(data.message || `Error HTTP ${response.status}`);
  return data;
}
function apiGet(path) { return apiRequest(path); }
function apiPost(path, body) { return apiRequest(path, { method: "POST", body: JSON.stringify(body) }); }
function apiPut(path, body) { return apiRequest(path, { method: "PUT", body: JSON.stringify(body) }); }
function apiDelete(path) { return apiRequest(path, { method: "DELETE" }); }
