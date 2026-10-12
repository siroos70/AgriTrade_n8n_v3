import os, json

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

files = walk(src)
files.sort()
print("TOTAL_FILES=" + str(len(files)))