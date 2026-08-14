#!/usr/bin/env python3
"""Smoke tests for the portfolio catalog and recommendation service."""
import json
import socket
import subprocess
import sys
import time
import unittest
from pathlib import Path
from urllib.request import urlopen


PROJECT = Path(__file__).resolve().parents[1]
SERVER = PROJECT / "Tools" / "catalog_server.py"


def free_port():
    with socket.socket() as sock:
        sock.bind(("127.0.0.1", 0))
        return sock.getsockname()[1]


class CatalogApiTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.port = free_port()
        cls.process = subprocess.Popen(
            [sys.executable, str(SERVER), str(cls.port)],
            cwd=PROJECT,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
        cls.base = f"http://127.0.0.1:{cls.port}"
        for _ in range(30):
            try:
                with urlopen(cls.base + "/health", timeout=0.2):
                    break
            except OSError:
                time.sleep(0.05)

    @classmethod
    def tearDownClass(cls):
        cls.process.terminate()
        cls.process.wait(timeout=3)

    def get_json(self, path):
        with urlopen(self.base + path, timeout=2) as response:
            return json.loads(response.read().decode("utf-8"))

    def test_health_route(self):
        self.assertEqual(self.get_json("/health")["status"], "ok")

    def test_catalog_route(self):
        payload = self.get_json("/catalog.json")
        self.assertGreaterEqual(len(payload["products"]), 5)

    def test_profile_recommendations(self):
        payload = self.get_json("/recommendations.json?profile=tech")
        self.assertEqual(payload["profile"], "tech")
        self.assertEqual(payload["products"][0]["id"], "workshop-kit")
        self.assertTrue(payload["model"])


if __name__ == "__main__":
    unittest.main(verbosity=2)
