import os, json, sys

src = r"F:\AgriTrade_n8n_v3_PDF_Integrated\AgriTrade_n8n_v3"

def walk(d):
    r = []
    try:
        entries = os.listdir(d)
    except Exception:
        return r
    for e in entries:
        p = os.path.join(d, e)
        try:
            is_dir = os.path.isdir(p)
        except Exception:
            continue
        if is_dir:
            if e in ("node_modules", ".git", "ascii"):
                continue
            r.extend(walk(p))
        elif e.endswith(".json"):
            r.append(p)
    return r

def fix_sql(q):
    out = ""
    i = 0
    n = len(q)
    while i < n:
        if q[i] == "{" and i+1 < n and q[i+1] == "{":
            j = q.find("}}", i)
            if j == -1:
                out += q[i:]
                break
            tok = q[i:j+2]
            tok = tok.replace("{{{$json.body.", "$json.body.")
            inner = tok[2:-2].strip()
            if inner.startswith("JSON.stringify"):
                replacement = "{{ " + inner + " }}"
            else:
                replacement = "{{" + inner + "}}"
            before = out[-1] if out else ""
            if before not in ("'", '"', "(", ","):
                out += "'"
            out += replacement
            after = q[j+2] if j+2 < n else ""
            if after not in ("'", '"', ",", ")"):
                out += "'"
            i = j + 2
        else:
            out += q[i]
            i += 1
    return out

files = walk(src)
files.sort()
seen = set()
fixed = 0
for f in files:
    with open(f, "r", encoding="utf-8") as fh:
        o = json.load(fh)
    if o.get("name") in seen:
        continue
    seen.add(o["name"])
    changed = False
    for node in o.get("nodes", []):
        if node.get("type") != "n8n-nodes-base.postgres":
            continue
        q = (node.get("parameters") or {}).get("query", "")
        if not q:
            continue
        orig = q
        q = fix_sql(q)
        if q != orig:
            node["parameters"]["query"] = q
            changed = True
            fixed += 1
    if changed:
        with open(f, "w", encoding="utf-8") as fh:
            json.dump(o, fh, ensure_ascii=False)

sys.stdout.write("FIXED=" + str(fixed) + "\n")