-- افزونه CRM و فروش حرفه‌ای بر اساس ابزارهای PDF آموزشی
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS target_markets (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), name TEXT NOT NULL, product TEXT, age_range TEXT, gender TEXT,
 education TEXT, location TEXT, lifestyle TEXT, values TEXT, interests TEXT, needs TEXT, buying_triggers TEXT,
 created_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS customer_profiles (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), name TEXT NOT NULL, market_id UUID REFERENCES target_markets(id) ON DELETE SET NULL,
 customer_type TEXT, needs TEXT, buying_pattern TEXT, motivation TEXT, objections TEXT, preferred_channel TEXT,
 notes TEXT, created_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS competitors (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), name TEXT NOT NULL, market TEXT, strengths TEXT, weaknesses TEXT,
 our_advantages TEXT, evidence TEXT, created_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS sales_plans (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), title TEXT NOT NULL, period_start DATE, period_end DATE,
 target_value NUMERIC DEFAULT 0, target_deals INTEGER DEFAULT 0, target_leads INTEGER DEFAULT 0,
 strategy TEXT, status TEXT NOT NULL DEFAULT 'فعال', created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS sales_actions (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), plan_id UUID REFERENCES sales_plans(id) ON DELETE CASCADE,
 title TEXT NOT NULL, description TEXT, owner TEXT, start_date DATE, due_date DATE,
 measure TEXT, target_value NUMERIC, status TEXT NOT NULL DEFAULT 'باز', priority TEXT NOT NULL DEFAULT 'متوسط', created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS sales_leads (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), name TEXT NOT NULL, phone TEXT, organization TEXT, role TEXT,
 lead_type TEXT NOT NULL DEFAULT 'خریدار', source TEXT, region TEXT, product_interest TEXT, quantity_interest_kg NUMERIC,
 budget NUMERIC, stage TEXT NOT NULL DEFAULT 'جدید', priority TEXT NOT NULL DEFAULT 'متوسط', score NUMERIC DEFAULT 0,
 customer_personality TEXT, needs TEXT, pain_points TEXT, objections TEXT, value_proposition TEXT,
 last_contact_at TIMESTAMPTZ, next_followup_at TIMESTAMPTZ, owner TEXT, status TEXT NOT NULL DEFAULT 'فعال', notes TEXT,
 created_at TIMESTAMPTZ NOT NULL DEFAULT now(), updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS lead_activities (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), lead_id UUID NOT NULL REFERENCES sales_leads(id) ON DELETE CASCADE,
 activity_type TEXT NOT NULL, channel TEXT, subject TEXT, summary TEXT, outcome TEXT,
 next_action TEXT, next_followup_at TIMESTAMPTZ, owner TEXT, created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS customer_personality_assessments (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), lead_id UUID REFERENCES sales_leads(id) ON DELETE CASCADE,
 personality TEXT NOT NULL, evidence TEXT, communication_style TEXT, recommended_approach TEXT, created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS discovery_questions (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), category TEXT NOT NULL, question TEXT NOT NULL, active BOOLEAN NOT NULL DEFAULT true,
 sort_order INTEGER DEFAULT 0
);
CREATE TABLE IF NOT EXISTS value_propositions (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), name TEXT NOT NULL, target_market_id UUID REFERENCES target_markets(id) ON DELETE SET NULL,
 customer_problem TEXT, promise TEXT, proof TEXT, usp TEXT, call_to_action TEXT, active BOOLEAN NOT NULL DEFAULT true,
 created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS sales_followups (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), lead_id UUID NOT NULL REFERENCES sales_leads(id) ON DELETE CASCADE,
 due_at TIMESTAMPTZ NOT NULL, channel TEXT DEFAULT 'تماس', purpose TEXT, script TEXT, status TEXT NOT NULL DEFAULT 'برنامه‌ریزی‌شده',
 completed_at TIMESTAMPTZ, created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS sales_presentations (
 id UUID PRIMARY KEY DEFAULT gen_random_uuid(), title TEXT NOT NULL, audience TEXT, problem TEXT, solution TEXT,
 method TEXT, capabilities TEXT, value_proposition TEXT, usp TEXT, faq TEXT, call_to_action TEXT,
 created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS idx_sales_leads_stage ON sales_leads(stage,status);
CREATE INDEX IF NOT EXISTS idx_sales_leads_followup ON sales_leads(next_followup_at) WHERE status='فعال';
CREATE INDEX IF NOT EXISTS idx_lead_activities_lead ON lead_activities(lead_id,created_at DESC);
CREATE INDEX IF NOT EXISTS idx_sales_actions_due ON sales_actions(due_date,status);
CREATE INDEX IF NOT EXISTS idx_followups_due ON sales_followups(due_at,status);

INSERT INTO discovery_questions(category,question,sort_order) VALUES
('نیاز','الان مهم‌ترین نیاز شما در خرید انجیر/انار چیست؟',10),
('نیاز','چه مقدار و در چه بازه زمانی نیاز دارید؟',20),
('کیفیت','حداقل کیفیت قابل قبول شما چیست؟',30),
('قیمت','محدوده قیمت هدف شما چقدر است؟',40),
('محدودیت','چه چیزی ممکن است باعث شود معامله را انجام ندهید؟',50),
('تصمیم‌گیری','چه کسی تصمیم نهایی خرید را می‌گیرد؟',60),
('زمان','اگر گزینه مناسب پیدا شود، چه زمانی آماده تصمیم هستید؟',70),
('ارزش','در انتخاب تأمین‌کننده، قیمت مهم‌تر است یا کیفیت/تحویل/اعتماد؟',80)
ON CONFLICT DO NOTHING;
