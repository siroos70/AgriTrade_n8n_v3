
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS sellers (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name TEXT NOT NULL,
  phone TEXT NOT NULL,
  region TEXT,
  notes TEXT,
  status TEXT NOT NULL DEFAULT 'فعال',
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS buyers (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name TEXT NOT NULL,
  phone TEXT NOT NULL,
  region TEXT,
  notes TEXT,
  status TEXT NOT NULL DEFAULT 'فعال',
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS offers (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  seller_id UUID NOT NULL REFERENCES sellers(id),
  product TEXT NOT NULL CHECK (product IN ('انجیر','انار')),
  variety TEXT,
  quantity_kg NUMERIC NOT NULL CHECK (quantity_kg > 0),
  quality TEXT,
  ready_date DATE,
  unit_price NUMERIC,
  packaging TEXT,
  location TEXT,
  status TEXT NOT NULL DEFAULT 'فعال',
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS demands (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  buyer_id UUID NOT NULL REFERENCES buyers(id),
  product TEXT NOT NULL CHECK (product IN ('انجیر','انار')),
  variety TEXT,
  quantity_kg NUMERIC NOT NULL CHECK (quantity_kg > 0),
  min_quality TEXT,
  max_unit_price NUMERIC,
  delivery_location TEXT,
  needed_date DATE,
  status TEXT NOT NULL DEFAULT 'فعال',
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS matches (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  offer_id UUID NOT NULL REFERENCES offers(id),
  demand_id UUID NOT NULL REFERENCES demands(id),
  score NUMERIC NOT NULL,
  reasons JSONB,
  status TEXT NOT NULL DEFAULT 'جدید',
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS deals (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  match_id UUID REFERENCES matches(id),
  seller_id UUID NOT NULL REFERENCES sellers(id),
  buyer_id UUID NOT NULL REFERENCES buyers(id),
  product TEXT NOT NULL,
  quantity_kg NUMERIC NOT NULL,
  unit_price NUMERIC NOT NULL,
  total_value NUMERIC NOT NULL,
  status TEXT NOT NULL DEFAULT 'مذاکره',
  agreed_at TIMESTAMPTZ,
  delivered_at TIMESTAMPTZ,
  settled_at TIMESTAMPTZ,
  notes TEXT,
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS commissions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  deal_id UUID NOT NULL REFERENCES deals(id),
  commission_type TEXT NOT NULL DEFAULT 'درصدی',
  rate NUMERIC,
  amount NUMERIC NOT NULL,
  status TEXT NOT NULL DEFAULT 'ثبت‌شده',
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_offers_product_status ON offers(product,status);
CREATE INDEX IF NOT EXISTS idx_demands_product_status ON demands(product,status);
CREATE INDEX IF NOT EXISTS idx_deals_status ON deals(status);
