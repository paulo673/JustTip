import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../ui/table'
import type { EmployeePayout } from '../../hooks/useTips'

interface PayoutTableProps {
  payouts: EmployeePayout[]
}

export function PayoutTable({ payouts }: PayoutTableProps) {
  if (payouts.length === 0) {
    return (
      <div className="bg-white rounded-xl border border-slate-200 p-12 shadow-sm text-center">
        <p className="text-slate-500">No payout data available for this week.</p>
        <p className="text-sm text-slate-400 mt-1">Enter daily tips above and click "Calculate & Save".</p>
      </div>
    )
  }

  return (
    <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow className="bg-slate-50">
            <TableHead className="font-semibold">Employee Name</TableHead>
            <TableHead className="font-semibold text-right">Total Hours</TableHead>
            <TableHead className="font-semibold text-right">Tip Share</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {payouts.map((payout, index) => (
            <TableRow key={payout.employeeName ?? index}>
              <TableCell className="font-medium">{payout.employeeName ?? '-'}</TableCell>
              <TableCell className="text-right text-slate-600">
                {(payout.totalHours ?? 0).toFixed(1)}h
              </TableCell>
              <TableCell className="text-right font-bold text-emerald-600">
                €{(payout.payoutAmount ?? 0).toFixed(2)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
