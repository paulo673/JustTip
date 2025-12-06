import { useState } from 'react'
import { startOfWeek, format } from 'date-fns'
import { Banknote, Clock } from 'lucide-react'
import { Navbar } from '../components/layout/Navbar'
import { WeekNavigator } from '../components/roster/WeekNavigator'
import { DailyInputGrid } from '../components/tips/DailyInputGrid'
import { PayoutTable } from '../components/tips/PayoutTable'
import { useTipsPayout, useDailyTips, useSaveTips } from '../hooks/useTips'
import type { DailyTipInput } from '../hooks/useTips'

export function TipsPage() {
  const [selectedDate, setSelectedDate] = useState(() => new Date())
  const weekStart = startOfWeek(selectedDate, { weekStartsOn: 1 })
  const weekKey = format(weekStart, 'yyyy-MM-dd')

  const { data, isLoading, isError, error } = useTipsPayout(selectedDate)
  const { data: dailyTipsData } = useDailyTips(selectedDate)
  const saveTipsMutation = useSaveTips()

  const handleSaveTips = (tips: DailyTipInput[]) => {
    saveTipsMutation.mutate(tips)
  }

  return (
    <div className="min-h-screen bg-slate-50">
      <Navbar activePage="tips" />

      <main className="max-w-7xl mx-auto px-6 py-8">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-2xl font-semibold text-slate-900">Tips Dashboard</h1>

          <WeekNavigator
            selectedDate={selectedDate}
            onDateChange={setSelectedDate}
          />

          <div className="w-[120px]" />
        </div>

        <section className="mb-8">
          <h2 className="text-lg font-medium text-slate-700 mb-4">Daily Tips Input</h2>
          <DailyInputGrid
            key={weekKey}
            selectedDate={selectedDate}
            existingTips={dailyTipsData}
            onSubmit={handleSaveTips}
            isSubmitting={saveTipsMutation.isPending}
          />
        </section>

        <section className="grid grid-cols-2 gap-4 mb-8">
          <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm">
            <div className="flex items-center gap-3 mb-2">
              <div className="w-10 h-10 rounded-lg bg-emerald-100 flex items-center justify-center">
                <Banknote className="h-5 w-5 text-emerald-600" />
              </div>
              <span className="text-sm font-medium text-slate-500">Total Tips</span>
            </div>
            <p className="text-3xl font-bold text-slate-900">
              €{(data?.totalWeeklyTips ?? 0).toFixed(2)}
            </p>
          </div>

          <div className="bg-white rounded-xl border border-slate-200 p-6 shadow-sm">
            <div className="flex items-center gap-3 mb-2">
              <div className="w-10 h-10 rounded-lg bg-blue-100 flex items-center justify-center">
                <Clock className="h-5 w-5 text-blue-600" />
              </div>
              <span className="text-sm font-medium text-slate-500">Total Hours</span>
            </div>
            <p className="text-3xl font-bold text-slate-900">
              {(data?.totalWeeklyHours ?? 0).toFixed(1)}h
            </p>
          </div>
        </section>

        <section>
          <h2 className="text-lg font-medium text-slate-700 mb-4">Employee Payouts</h2>

          {isLoading && (
            <div className="bg-white rounded-xl border border-slate-200 p-12 shadow-sm">
              <div className="flex items-center justify-center">
                <div className="animate-spin rounded-full h-8 w-8 border-2 border-slate-200 border-t-slate-900" />
                <span className="ml-4 text-slate-500 font-medium">Loading payouts...</span>
              </div>
            </div>
          )}

          {isError && (
            <div className="bg-red-50 border border-red-200 rounded-xl p-6">
              <p className="text-red-600 font-medium">
                Error loading payouts: {error?.message || 'Unknown error'}
              </p>
            </div>
          )}

          {!isLoading && !isError && (
            <PayoutTable payouts={data?.employeePayouts ?? []} />
          )}
        </section>
      </main>
    </div>
  )
}
