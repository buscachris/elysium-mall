#!/usr/bin/env python3
"""Serve the Elysium Mall demo catalog and recommendation API."""
from http.server import BaseHTTPRequestHandler, HTTPServer
from pathlib import Path
import json
import sys
from urllib.parse import parse_qs, urlparse

PROJECT = Path(__file__).resolve().parents[1]
CATALOG = PROJECT / "Assets" / "Resources" / "catalog.json"


class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        request = urlparse(self.path)
        route = request.path.rstrip("/") or "/"
        if route == "/health":
            self._send_json({"service": "elysium-mall-catalog", "status": "ok"})
            return

        catalog = json.loads(CATALOG.read_text(encoding="utf-8"))
        if route == "/catalog.json":
            self._send_json(catalog)
            return

        if route == "/recommendations.json":
            profile = parse_qs(request.query).get("profile", ["explorer"])[0].lower()
            self._send_json(build_recommendations(catalog, profile))
            return

        self._send_json({"service": "elysium-mall-catalog", "route": "/catalog.json"}, 404)

    def _send_json(self, value, status=200):
        body = json.dumps(value).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Cache-Control", "no-store")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def log_message(self, format, *args):
        print(format % args)


PROFILE_ORDER = {
    "explorer": ["runner-shoes", "ring-display", "chao-care-pack"],
    "style": ["guardian-jacket", "runner-shoes", "chao-care-pack"],
    "tech": ["workshop-kit", "runner-shoes", "ring-display"],
}

PROFILE_RATIONALE = {
    "explorer": "Prioritized movement, discovery, and companion-friendly products.",
    "style": "Prioritized apparel and products that support a bold visitor identity.",
    "tech": "Prioritized maker tools and products for a hands-on technical visitor.",
}


def build_recommendations(catalog, profile):
    """Return deterministic demo recommendations behind an AI-service-shaped API."""
    order = PROFILE_ORDER.get(profile, PROFILE_ORDER["explorer"])
    by_id = {product["id"]: product for product in catalog.get("products", [])}
    products = [by_id[product_id] for product_id in order if product_id in by_id]
    return {
        "profile": profile if profile in PROFILE_ORDER else "explorer",
        "rationale": PROFILE_RATIONALE.get(profile, PROFILE_RATIONALE["explorer"]),
        "model": "elysium-demo-ranker-v1",
        "products": products,
    }


if __name__ == "__main__":
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 8766
    print(f"Catalog API: http://127.0.0.1:{port}/catalog.json")
    HTTPServer(("127.0.0.1", port), Handler).serve_forever()
