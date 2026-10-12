import os, json, sys
base = r"F:\AgriTrade_n8n_v3_PDF_Integrated\AgriTrade_n8n_v3\13_فروش_حرفه‌ای_برگرفته_از_PDF"
for fn in os.listdir(base):
    if "\u0631\u0642\u06cc\u0628" in fn and fn.endswith(".json"):
        p = os.path.join(base, fn)
        with open(p, "r", encoding="utf-8") as fh:
            o = json.load(fh)
        sys.stdout.write("FILE=" + fn + "\n")
        for n in o.get("nodes", []):
            if n.get("type") == "n8n-nodes-base.postgres":
                q = n["parameters"]["query"]
                sys.stdout.write("QUERY=" + q + "\n")
                i = q.find("JSON.stringify")
                sys.stdout.write("AROUND=" + repr(q[i-5:i+50]) + "\n")